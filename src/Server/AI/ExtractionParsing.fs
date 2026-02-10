namespace LibraryDiscovery.AI

open System.Text.Json
open LibraryDiscovery.Domain
open LibraryDiscovery.Utils.JsonExtractor


// Helps to parse the Json returned by different AI response strucuture.
module ExtractionParsing =

    let tryExtractGeminiText (doc: JsonDocument) =
        try
            ((doc.RootElement.GetProperty("candidates")[0]).GetProperty("content").GetProperty("parts")[0]).GetProperty("text").GetString()
            |> fun x -> Ok x
        with ex -> Error $"Failed to extract Gemini text: {ex.Message}"

    let tryGetChatGptText (doc: JsonDocument) : Result<string, string> =
        try
            LibraryDiscovery.Utils.JsonExtractor.tryStringAt
                [ "choices"; "0"; "message"; "content" ]
                doc.RootElement
            |> Option.map Ok
            |> Option.defaultValue (Error "ChatGPT returned empty choices[0].message.content")
        with ex ->
            Error $"Failed to extract ChatGPT text: {ex.Message}"

    // this is looking to parse our custom shape out that we defined and request in the prompts.
    // if extending this or the types, the other must be changed. for this reason, may make more sense to 
    // further abstract the prompt out to have the json shape and type be one in the same to avoid this. 
    let parseExtracted (jsonText: string) : Result<ExtractedQuery, string> =
        try
            System.Console.WriteLine $"JSON: {jsonText}"
            use doc = JsonDocument.Parse(jsonText)
            let root: JsonElement = doc.RootElement

            let optString (name: string) =
                tryProp name root |> Option.bind tryString

            let optInt (name: string) =
                tryProp name root |> Option.bind tryInt

            let keywords =
                tryProp "keywords" root
                |> Option.filter (fun v -> v.ValueKind = JsonValueKind.Array)
                |> Option.map (fun arr ->
                    arr.EnumerateArray()
                    |> Seq.choose tryString
                    |> Seq.toList)
                |> Option.defaultValue []

            Ok {
                TitleOpt  = optString "titleOpt"
                AuthorOpt = optString "authorOpt"
                Keywords  = keywords
                YearOpt   = optInt "yearOpt"
                Ambiguity = optString "ambiguity"
            }
        with ex ->
            Error $"Failed to parse extracted JSON: {ex.Message}"
