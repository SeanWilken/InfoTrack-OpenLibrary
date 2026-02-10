namespace LibraryDiscovery.AI

open System


module Config =

    // DU / Enum for matching on what provider to use
    type AiProvider =
        | Gemini = 0
        | ChatGpt = 1

    // Options relevant to Gemini configuration
    type GeminiOptions = {
        ApiKey  : string
        Models  : string list
        BaseUrl : string
    }

    // Options relevant to ChatGPT configuration
    type ChatGptOptions = {
        ApiKey      : string
        Model       : string
        BaseUrl     : string // allow override; default OpenAI
        Organization: string option
        Project     : string option
    }

    // Options to hold configurations for Ai endpoint configuration
    type AiOptions = {
        Provider : AiProvider
        Gemini   : GeminiOptions option
        ChatGpt  : ChatGptOptions option
    }

    // Build a config from the .env file at the repo root, to figure out the
    // API key for Gemini and for ChatGPT
    module AiConfig =

        let private env (name: string) =
            Environment.GetEnvironmentVariable(name)

        let private envOpt (name: string) =
            match env name with
            | null | "" -> None
            | v -> Some v

        // 'constructor' for AiOptions, using .env 
        let load () : AiOptions =

            let has name =
                let v = System.Environment.GetEnvironmentVariable(name)
                not (System.String.IsNullOrWhiteSpace v)

            printfn "AI_PROVIDER set: %b" (has "AI_PROVIDER")
            printfn "GEMINI_API_KEY set: %b" (has "GEMINI_API_KEY")
            printfn "OPENAI_API_KEY set: %b" (has "OPENAI_API_KEY")

            // Provider selection:
            // AI_PROVIDER = "gemini" | "chatgpt"
            let provider =
                match envOpt "AI_PROVIDER" |> Option.map (fun s -> s.Trim().ToLowerInvariant()) with
                | Some "chatgpt" | Some "openai" -> AiProvider.ChatGpt
                | _ -> AiProvider.Gemini

            let gemini =
                match envOpt "GEMINI_API_KEY" with
                | Some key ->
                    Some {
                        ApiKey = key
                        BaseUrl =  envOpt "GEMINI_BASE_URL" |> Option.defaultValue "https://generativelanguage.googleapis.com/v1"
                        Models = 
                            envOpt "GEMINI_MODELS"
                            |> Option.map (fun models -> 
                                models.Split(",") 
                                |> Array.map (fun model -> model.Trim() ) 
                                |> Array.toList 
                            )
                            |> Option.defaultValue [ "gemini-1.5-flash" ]
                    }
                | None -> None

            let chat =
                match envOpt "OPENAI_API_KEY" with
                | Some key ->
                    Some {
                        ApiKey = key
                        Model = envOpt "OPENAI_MODEL" |> Option.defaultValue "gpt-4o-mini"
                        BaseUrl = envOpt "OPENAI_BASE_URL" |> Option.defaultValue "https://api.openai.com/v1"
                        Organization = envOpt "OPENAI_ORG"
                        Project = envOpt "OPENAI_PROJECT"
                    }
                | None -> None

            {
                Provider = provider
                Gemini = gemini
                ChatGpt = chat
            }
