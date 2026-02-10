module Server

open System
open System.Net.Http
open SAFE
open Saturn
open Shared
open Microsoft.Extensions.DependencyInjection
open LibraryDiscovery.AI.QueryExtractor
open LibraryDiscovery.Endpoints.Discovery


let webApp = Api.make LibraryDiscoveryApi.api

let app = application {

    // DI
    service_config (fun services ->
        // HttpClient factory + named clients
        services.AddHttpClient("default") |> ignore
        services.AddHttpClient("ai") |> ignore

        // Register extractor as singleton via factory
        services.AddSingleton<IQueryExtractor>(fun sp ->
            LibraryDiscovery.AI.ExtractorFactory.create sp
        ) |> ignore

        // Register discovery service
        services.AddSingleton<IDiscoveryService, DiscoveryServiceImpl>() |> ignore

        services
    )

    use_router webApp
    memory_cache
    use_static "public"
    use_gzip
}

[<EntryPoint>]
let main _ =
    // .env located at repo root, see .env.example
    DotNetEnv.Env.TraversePath().Load() |> ignore
    run app
    0