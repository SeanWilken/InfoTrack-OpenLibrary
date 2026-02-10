namespace LibraryDiscovery.AI

open System
open System.Net.Http
open Microsoft.Extensions.DependencyInjection
open LibraryDiscovery.AI.Config
open LibraryDiscovery.AI.Gemini
open LibraryDiscovery.AI.ChatGPT
open LibraryDiscovery.AI.QueryExtractor
open LibraryDiscovery.AI.CompositeExtractor
open LibraryDiscovery.AI.FallbackExtractor

module ExtractorFactory =

    // Extractor factory to spawn a Query extractor. The extractor first tries to parse the LLM response
    // and if it fails to find anything or cannot parse the result, it will use the composites fallback extractor
    let create (sp: IServiceProvider) : IQueryExtractor =
        let httpFactory = sp.GetRequiredService<IHttpClientFactory>()
        let http = httpFactory.CreateClient("ai")

        let cfg = AiConfig.load()
        let fallback = FallbackExtractor() :> IQueryExtractor

        match cfg.Provider with
        | AiProvider.ChatGpt ->
            match cfg.ChatGpt with
            | Some chat ->
                let primary = ChatGptExtractor(http, chat) :> IQueryExtractor
                CompositeExtractor(primary, fallback) :> IQueryExtractor
            | None -> fallback

        | _ ->
            match cfg.Gemini with
            | Some gem ->
                let primary = GeminiExtractor(http, gem) :> IQueryExtractor
                CompositeExtractor(primary, fallback) :> IQueryExtractor
            | None -> fallback
