namespace LibraryDiscovery.AI

open System.Threading.Tasks
open LibraryDiscovery.Domain
open LibraryDiscovery.AI.QueryExtractor
open System
open System.Threading.Tasks
open LibraryDiscovery.Domain
open LibraryDiscovery.Utils.Normalize

module FallbackExtractor =

    // fallback extractor if we can't get anything useful from the AI response.
    type FallbackExtractor() =
        interface IQueryExtractor with
            member _.Extract(rawQuery: string) =
                task {
                    let q = rawQuery.Trim()
                    if String.IsNullOrWhiteSpace q then
                        return
                            (
                                { TitleOpt=None; AuthorOpt=None; Keywords=[]; YearOpt=None; Ambiguity=None },
                                ["Empty query."]
                            )
                    else
                        let yearOpt = tryExtractYear q

                        let toks =
                            normalizeText q
                            |> fun s -> s.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            |> Array.toList

                        let titleOpt, authorOpt, keywords =
                            match toks with
                            | [] -> None, None, []
                            | [one] -> None, Some one, []
                            | a::b::rest when rest.IsEmpty -> Some (a + " " + b), None, []
                            | _ -> None, None, toks

                        return
                            ({ TitleOpt=titleOpt; AuthorOpt=authorOpt; Keywords=keywords; YearOpt=yearOpt; Ambiguity=None },
                            ["Used fallback extractor (LLM unavailable or failed)."])
                }

module CompositeExtractor =

    type CompositeExtractor(primary: IQueryExtractor, fallback: IQueryExtractor) =
        interface IQueryExtractor with
            member _.Extract(q: string) =
                task {
                    let! (ex1, msgs1) = primary.Extract(q)

                    let looksEmpty =
                        ex1.TitleOpt.IsNone
                        && ex1.AuthorOpt.IsNone
                        && ex1.YearOpt.IsNone
                        && ex1.Keywords.IsEmpty

                    if looksEmpty then
                        let! (ex2, msgs2) = fallback.Extract(q)
                        return ex2, (msgs1 @ msgs2)
                    else
                        return ex1, msgs1
                }
