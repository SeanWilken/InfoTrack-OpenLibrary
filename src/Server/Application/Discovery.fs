namespace LibraryDiscovery.Application

open System
open System.Threading.Tasks
open Shared.LibraryDiscovery
open LibraryDiscovery.AI
open LibraryDiscovery.Utils.Normalize
open LibraryDiscovery.Core.OpenLibraryClient
open LibraryDiscovery.Domain

module Discovery =
    open LibraryDiscovery.AI.QueryExtractor
    open LibraryDiscovery.Domain.OpenLibrary.Dto

    // ---------- Helpers ----------

    let private coverUrlFromCoverId (coverId: int option) =
        coverId |> Option.map (fun id -> $"https://covers.openlibrary.org/b/id/{id}-M.jpg")

    let private bestYear (firstPublishYear: int option) (workFirstPublishDate: string option) =
        // Prefer explicit year if you have it
        match firstPublishYear with
        | Some y -> Some y
        | None ->
            match workFirstPublishDate with
            | None -> None
            | Some s ->
                // naive: find a 4-digit year in publish_date
                tryExtractYear s

    let private titleExactNear (hint: string) (title: string) (subtitle: string option) =
        let hintN = normalizeText hint
        let hintMain = mainTitle hintN

        let titleN = normalizeText title
        let titleMain = mainTitle titleN

        let subtitleN = subtitle |> Option.map normalizeText

        let exact =
            hintN = titleN
            || hintMain = titleMain
            || (subtitleN |> Option.exists (fun sub -> hintN = (titleMain + " " + sub)))

        let near =
            let a = tokens hintN
            let b = tokens titleN
            jaccard a b >= 0.6

        exact, near

    let private authorJaccardMatch (hint: string) (authors: string list) =
        let hintToks = tokens hint
        authors
        |> List.exists (fun a ->
            let aToks = tokens a
            jaccard hintToks aToks >= 0.5
        )

    let private contributorMatch (hint: string) (contributors: string list) =
        let hn = normalizeText hint
        contributors
        |> List.exists (fun c -> normalizeText c |> fun cn -> cn.Contains(hn, StringComparison.Ordinal))

    let mkExplanation
        (ex: ExtractedQuery)
        (rw: ResolvedWork)
        (signals: CandidateSignals)
        =

        let dominantReason =
            if signals.TitleExact && signals.PrimaryAuthorMatch then
                "Exact title match with canonical work and matching primary author"
            elif signals.TitleExact then
                "Exact title match with canonical work"
            elif signals.TitleNear && signals.PrimaryAuthorMatch then
                "Near title match with matching primary author"
            elif signals.PrimaryAuthorMatch then
                "Matched based on primary author"
            elif signals.ContributorMatch then
                "Author appears as a contributor rather than primary author"
            else
                "Matched based on Open Library relevance"

        let qualifier =
            match ex.YearOpt, rw.FirstYear with
            | Some y1, Some y2 when y1 = y2 ->
                Some $"publication year {y2} matches query"
            | _ ->
                None

        let keywordNote =
            match ex.Keywords with
            | [] -> None
            | ks ->
                let shown = ks |> List.truncate 3 |> String.concat ", "
                Some $"keywords aligned: {shown}"

        [ Some dominantReason; qualifier; keywordNote ]
        |> List.choose id
        |> String.concat "; "
        |> fun s -> s + "."


    // Ranking heirarchy per requirements
    let private scoreBucket (ex: ExtractedQuery) (s: CandidateSignals) =
        // Matching hierarchy
        if s.TitleExact && s.PrimaryAuthorMatch then 500
        elif s.TitleExact && s.ContributorMatch then 400
        elif s.TitleNear  && s.PrimaryAuthorMatch then 300
        elif ex.AuthorOpt.IsSome && s.PrimaryAuthorMatch then 200
        else 100

    // ---------- Canonical resolution ----------
    // We convert OpenLibrary search docs -> ResolvedWork by fetching work + authors.
    // This ensures we have primary authors from the canonical work record.

    // Try to resolve an item, if it fails, skip it so the batch doesn't fail
    // BETTER: fallback handling of exception
    let private tryResolveOneWork (ol: OpenLibraryClient) (doc: SearchDoc) =
        task {
            try
                let workKey = doc.key  // e.g. "/works/OLxxxxW"
                let! wd = ol.GetWork(workKey)

                // resolve canonical authors
                let authorIds =
                    wd.authors
                    |> Array.choose (fun a -> a.author.key |> Option.ofObj)
                    |> Array.distinct

                let! authors =
                    authorIds
                    |> Array.map (fun id -> task {
                        let! ad = ol.GetAuthor(id)
                        return (id, ad.name)
                    })
                    |> Task.WhenAll

                let firstYear =
                    match doc.first_publish_year with
                    | Some y -> Some y
                    | None -> tryExtractYear (defaultArg wd.first_publish_date "")

                return Some {
                    WorkKey = workKey
                    Title = wd.title
                    Subtitle = wd.subtitle
                    FirstYear = firstYear
                    PrimaryAuthors = authors |> Array.toList
                    Contributors = doc.author_name |> Array.toList
                    CoverUrl = doc.cover_i |> Option.map (fun id -> $"https://covers.openlibrary.org/b/id/{id}-M.jpg")
                }
            with ex ->
                // swallow per-item failure
                return None
        }

    let private dedupeSearchDocsToWorks (docs: SearchDoc[]) =
        // Deduplicate by doc.key (work key)
        docs
        |> Array.distinctBy (fun d -> d.key)
        |> Array.truncate 10
        |> Array.toList

    // ---------- Public pipeline ----------
    let discover (extractor: IQueryExtractor) (ol: OpenLibraryClient) (rawQuery: string) : Task<LibrarySearchResponse> =
        task {
            // 1) Extract hypothesis using AI model configured in .env
            let! (extracted, msgs1) = extractor.Extract(rawQuery)

            // If extractor didn't find year but raw query includes one, capture it
            let extracted =
                if extracted.YearOpt.IsNone then
                    { extracted with YearOpt = tryExtractYear rawQuery }
                else extracted

            // 2) Build OpenLibrary query string
            let queryForSearch =
                let parts =
                    [
                        extracted.TitleOpt |> Option.defaultValue ""
                        extracted.AuthorOpt |> Option.defaultValue ""
                        (String.Join(" ", extracted.Keywords))
                        extracted.YearOpt |> Option.map string |> Option.defaultValue ""
                    ]
                    |> List.map (fun s -> s.Trim())
                    |> List.filter (fun s -> not (String.IsNullOrWhiteSpace s))

                if parts.IsEmpty then rawQuery else String.Join(" ", parts)

            // 3) Search
            let! sr = ol.Search(queryForSearch)
            let docs = dedupeSearchDocsToWorks sr.docs

            // 4) Resolve canonical work details + primary authors
            let! resolved =
                docs
                |> List.map (tryResolveOneWork ol)
                |> Task.WhenAll

            let resolved = resolved |> Array.choose id |> Array.toList

            // 5) Rank + explain
            let candidates =
                resolved
                |> List.map (fun rw ->
                    let titleExact, titleNear =
                        match extracted.TitleOpt with
                        | Some t -> titleExactNear t rw.Title rw.Subtitle
                        | None -> false, false

                    let primaryAuthorNames = rw.PrimaryAuthors |> List.map snd

                    let primaryAuthorMatch =
                        match extracted.AuthorOpt with
                        | Some a -> authorJaccardMatch a primaryAuthorNames
                        | None -> false

                    let contributorMatchSignal =
                        match extracted.AuthorOpt with
                        | Some a -> contributorMatch a rw.Contributors
                        | None -> false

                    let yearMatch =
                        match extracted.YearOpt, rw.FirstYear with
                        | Some y1, Some y2 -> y1 = y2
                        | _ -> false

                    let signals = {
                        TitleExact = titleExact
                        TitleNear = titleNear
                        PrimaryAuthorMatch = primaryAuthorMatch
                        ContributorMatch = contributorMatchSignal
                        YearMatch = yearMatch
                    }

                    let explanation = mkExplanation extracted rw signals

                    {
                        WorkKey = rw.WorkKey
                        Title = rw.Title
                        PrimaryAuthors = primaryAuthorNames
                        FirstYear = rw.FirstYear
                        CoverUrl = rw.CoverUrl
                        Signals = signals
                        Explanation = explanation
                    }
                )
                |> List.sortByDescending (fun c ->
                    let bucket = scoreBucket extracted c.Signals
                    let yearBoost = if c.Signals.YearMatch then 1 else 0
                    bucket, yearBoost
                )
                |> List.truncate 5

            // 6) Map to Shared DTOs
            let normalizedQueryDto : ExtractedQueryDetailDto = {
                titleOpt = extracted.TitleOpt
                authorOpt = extracted.AuthorOpt
                keywords = extracted.Keywords
                yearOpt = extracted.YearOpt
                ambiguity = extracted.Ambiguity
            }

            let candidateDtos : CandidateDto list =
                candidates
                |> List.map (fun c ->
                    {
                        workTitle = c.Title
                        workId = c.WorkKey
                        author = String.Join(", ", c.PrimaryAuthors)
                        authorId = List.tryHead c.PrimaryAuthors |> Option.defaultValue ""
                        firstPublishYear = c.FirstYear
                        coverUrl = c.CoverUrl
                        explanation = c.Explanation
                    })

            return {
                normalizedQuery = normalizedQueryDto
                candidates = candidateDtos
                messages = msgs1
            }
        }
