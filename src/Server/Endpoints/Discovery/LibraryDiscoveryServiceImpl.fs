namespace LibraryDiscovery.Endpoints.Discovery

open System.Threading.Tasks
open System.Net.Http
open Shared.LibraryDiscovery
open LibraryDiscovery.AI
open LibraryDiscovery.Application
open LibraryDiscovery.Core.OpenLibraryClient
open LibraryDiscovery.AI.QueryExtractor

type DiscoveryServiceImpl(httpClientFactory: IHttpClientFactory, extractor: IQueryExtractor) =

    interface IDiscoveryService with
        member _.Discover(query: string) : Task<LibrarySearchResponse> =
            task {
                let http = httpClientFactory.CreateClient("default")
                let ol = OpenLibraryClient(http)
                return! Discovery.discover extractor ol query
            }
