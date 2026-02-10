namespace LibraryDiscovery.Core

open System
open System.Net.Http
open System.Text.Json
open System.Threading.Tasks
open LibraryDiscovery.Domain
open LibraryDiscovery.Domain.OpenLibrary.Dto

module OpenLibraryClient =

    type OpenLibraryClient(http: HttpClient) =

        static let jsonOptions =
            JsonSerializerOptions(PropertyNameCaseInsensitive = true)

        static member private getJson<'T> (client: HttpClient) (url: string) =
            task {
                use! resp = client.GetAsync(url)
                resp.EnsureSuccessStatusCode() |> ignore
                let! stream = resp.Content.ReadAsStreamAsync()
                let! data = JsonSerializer.DeserializeAsync<'T>(stream, jsonOptions)
                return data
            }

        member _.Search(q: string) : Task<SearchResponse> =
            let url =
                $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(q)}&limit=20"
            OpenLibraryClient.getJson<SearchResponse> http url

        member _.SearchByTitleAuthor(title: string, author: string option) : Task<SearchResponse> =
            let q =
                match author with
                | Some a when not (String.IsNullOrWhiteSpace a) -> $"title:{title} author:{a}"
                | _ -> $"title:{title}"
            let url =
                $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(q)}&limit=20"
            OpenLibraryClient.getJson<SearchResponse> http url

        member _.GetWork(workKey: string) : Task<WorkDetail> =
            // workKey like "/works/OL123W"
            let url = $"https://openlibrary.org{workKey}.json"
            OpenLibraryClient.getJson<WorkDetail> http url

        member _.GetAuthor(authorKey: string) : Task<AuthorDetail> =
            // authorKey like "/authors/OL123A"
            let url = $"https://openlibrary.org{authorKey}.json"
            OpenLibraryClient.getJson<AuthorDetail> http url
