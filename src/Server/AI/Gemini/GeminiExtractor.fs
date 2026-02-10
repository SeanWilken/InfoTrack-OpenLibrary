namespace LibraryDiscovery.AI.Gemini

open System
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open System.Threading.Tasks
open System.Text.Json
open LibraryDiscovery.AI.Config
open LibraryDiscovery.Domain
open LibraryDiscovery.AI.QueryExtractor
open System.Net
open LibraryDiscovery.AI

type GeminiExtractor(http: HttpClient, opts: GeminiOptions) =

    let prompt = """
You are a librarian query parser. Convert the raw text blob into JSON with:
{ "titleOpt": string|null, "authorOpt": string|null, "keywords": string[], "yearOpt": number|null, "ambiguity": string|null }
Rules:
- Output JSON ONLY.
- Do not invent facts. If unsure, use null or ambiguity.
- If a 4-digit year appears, put it in yearOpt.
"""

    let buildBody (raw: string) =
        JsonSerializer.Serialize(
            {| contents =
                [|
                    {| role = "user"
                       parts = [| {| text = prompt + "\n\nInput: " + raw + "\nOutput:" |} |]
                    |}
                |]
            |}
            ,JsonSerializerOptions(PropertyNameCaseInsensitive = true)
        )


    let callGeminiModel
        (opts: GeminiOptions)
        (model: string)
        (rawQuery: string) =

        let url =
            $"{opts.BaseUrl.TrimEnd('/')}beta/models/{model}:generateContent?key={opts.ApiKey}"

        let body = buildBody rawQuery

        LibraryDiscovery.Utils.Http.retryAsync 3 250 (fun () ->
            task {
                use req = new HttpRequestMessage(HttpMethod.Post, url)
                req.Content <- new StringContent(body, Encoding.UTF8, "application/json")

                use! resp = http.SendAsync(req)
                let! text = resp.Content.ReadAsStringAsync()

                if resp.IsSuccessStatusCode then
                    return Ok text
                else
                    return Error (resp.StatusCode, text)
            })

    let rec tryModels
        (models: string list)
        (errors: string list)
        (rawQuery: string) =
            task {
                match models with
                | [] ->
                    return {
                        TitleOpt = None
                        AuthorOpt = None
                        Keywords = []
                        YearOpt = None
                        Ambiguity = Some "Gemini unavailable"
                    }, List.rev errors

                | model :: rest ->
                    match! callGeminiModel opts model rawQuery with
                    | Ok body ->
                        use doc = JsonDocument.Parse body
                        match ExtractionParsing.tryExtractGeminiText doc with
                        | Ok ex -> 
                            match ExtractionParsing.parseExtracted ex with
                            | Ok extracted ->
                                return extracted, [ $"Gemini model used: {model}" ]
                            | Error e ->
                                return! 
                                    tryModels
                                        rest
                                        ($"Gemini parse failed ({model}): {e}" :: errors)
                                        rawQuery
                        | Error er ->  
                            return! 
                                tryModels
                                    rest
                                    ($"Gemini parse failed ({model}): {er}" :: errors)
                                    rawQuery

                    | Error (HttpStatusCode.NotFound, _) ->
                        return! tryModels
                            rest
                            ($"Gemini model not supported ({model})" :: errors)
                            rawQuery

                    | Error (status, body) ->
                        return! tryModels
                            rest
                            ($"Gemini call failed ({model}): {(int status)} {body}" :: errors)
                            rawQuery
            }


    interface IQueryExtractor with
        member _.Extract(rawQuery: string) =
            if String.IsNullOrWhiteSpace rawQuery then
                task {
                    return { 
                        TitleOpt = None
                        AuthorOpt = None
                        Keywords = []
                        YearOpt = None
                        Ambiguity = Some "No query supplied" 
                    }, [ $"You need to submit some combination of author, title, or keywords " ]
                }
            else
                let models =
                    opts.Models
                    |> List.map (fun x -> x.Trim())
                    |> List.filter (String.IsNullOrWhiteSpace >> not)

                tryModels models [] rawQuery
