namespace LibraryDiscovery.Core

// System Classes
open System
open System.Collections.Generic
open System.Threading.Tasks

// Comes from Server/Core/OpenLibraryClient.fs
open LibraryDiscovery.Core.OpenLibraryClient
// Comes from Server/Utils/Normalize.fs
open LibraryDiscovery.Utils.Normalize

// These both come from Server/Domain/Domain.fs
// Not shared as this is the servers concern
// // Internal Types
open LibraryDiscovery.Domain
// API Types for OpenLibrary API
open LibraryDiscovery.Domain.OpenLibrary.Dto


module OpenLibraryCanonicalize =

    let coverUrlFromCoverI (cover_i: int option) =
        cover_i |> Option.map (fun id -> $"https://covers.openlibrary.org/b/id/{id}-M.jpg")

    /// Work keys from search docs may be work keys or edition keys.
    /// Many search docs return "/works/OL..W" already. If you see "/books/OL..M", you can still keep it but prefer works.
    let isWorkKey (k: string) =
        k.StartsWith("/works/", StringComparison.OrdinalIgnoreCase)

    /// Dedupe to unique work keys, keep best representative doc per work.
    let dedupeToWorks (docs: SearchDoc[]) : SearchDoc list =
        docs
        |> Array.filter (fun d -> not (String.IsNullOrWhiteSpace d.key))
        |> Array.filter (fun d -> isWorkKey d.key) // for v1: keep only works
        |> Array.groupBy (fun d -> d.key)
        |> Array.map (fun (_k, group) ->
            // pick representative: prefer one with publish year, cover
            group
            |> Array.sortByDescending (fun d ->
                (if d.first_publish_year.IsSome then 1 else 0),
                (if d.cover_i.IsSome then 1 else 0))
            |> Array.head
        )
        |> Array.toList

    /// Resolve work detail + primary authors
    let resolveWorks
        (ol: OpenLibraryClient)
        (maxWorks: int)
        (docs: SearchDoc list)
        : Task<ResolvedWork list> =
        task {
            let docs = docs |> List.truncate maxWorks

            // request-scoped caches
            let authorCache = Dictionary<AuthorId, string>() // authorKey -> name
            // let workCache = Dictionary<string, WorkDetail>() // workKey -> detail
            let workCache = Dictionary<string, WorkDetail>() // workKey -> detail

            let! resolved =
                docs
                |> List.map (fun doc ->
                    task {
                        let workKey = doc.key

                        let! work =
                            task {
                                match workCache.TryGetValue(workKey) with
                                | true, w -> return w
                                | _ ->
                                    let! w = ol.GetWork(workKey)
                                    workCache.[workKey] <- w
                                    return w
                            }

                        // Resolve author names
                        let! authorNames =
                            work.authors
                            |> Array.map (fun aRef ->
                                task {
                                    let aKey = aRef.author.key
                                    let authorId = AuthorId aKey
                                    match authorCache.TryGetValue(aKey) with
                                    | true, cachedName -> return authorId, cachedName
                                    | _ ->
                                        let! a = ol.GetAuthor(aKey)
                                        authorCache.[authorId] <- a.name
                                        return authorId, a.name
                                }
                            )
                            |> Task.WhenAll

                        let contributors =
                            doc.author_name
                            |> Array.toList
                            |> List.map normalizeText

                        return {
                            WorkKey        = work.key
                            Title          = work.title
                            Subtitle       = work.subtitle
                            FirstYear      = doc.first_publish_year
                            PrimaryAuthors = authorNames |> Array.toList
                            CoverUrl       = coverUrlFromCoverI doc.cover_i
                            Contributors   = contributors
                        }
                    }
                )
                |> Task.WhenAll

            return resolved |> Array.toList
        }


module DiscoverPipeline =

    open System.Threading.Tasks


    let searchAndResolve (ol: OpenLibraryClient) (q: string) =
        task {
            let! sr = ol.Search(q)
            let docs = OpenLibraryCanonicalize.dedupeToWorks sr.docs
            let! works = OpenLibraryCanonicalize.resolveWorks ol 10 docs
            return works
        }