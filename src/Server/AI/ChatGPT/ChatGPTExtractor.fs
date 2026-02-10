namespace LibraryDiscovery.AI.ChatGPT

open System
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open System.Threading.Tasks
open LibraryDiscovery.AI.Config
open LibraryDiscovery.Domain
open System.Text.Json
open LibraryDiscovery.AI.QueryExtractor
open LibraryDiscovery.Utils.JsonExtractor
open System.Net
open LibraryDiscovery.AI

type ChatGptExtractor(http: HttpClient, opts: ChatGptOptions) =

    // Tune for ChatGPT, not shared prompt as results vary with different prompting and LLM's
    let prompt = """
You are a librarian query parser. Convert the raw text blob into JSON with:
{ "titleOpt": string|null, "authorOpt": string|null, "keywords": string[], "yearOpt": number|null, "ambiguity": string|null }
Rules:
- Output JSON ONLY.
- Do not invent facts. If unsure, use null or ambiguity.
- If a 4-digit year appears, put it in yearOpt.
"""

    // Shape for making a request to OpenAI
    let buildBody (raw: string) =
        JsonSerializer.Serialize(
            {| model = opts.Model
               temperature = 0
               messages =
                 [|
                   {| role = "system"; content = prompt |}
                   {| role = "user"; content = raw |}
                 |]
            |}
            , JsonSerializerOptions(PropertyNameCaseInsensitive = true)
        )

    // Actually call the API, has a functional wrapper around it for retries depending on
    // the status code returned. See src/Server/Utils/Http.fs for the retryAsync function
    let callChatGptModel
        (opts: ChatGptOptions)
        (model: string)
        (rawQuery: string) =

        let url = opts.BaseUrl.TrimEnd('/') + "/chat/completions"

        let body = buildBody rawQuery

        LibraryDiscovery.Utils.Http.retryAsync 3 250 (fun () ->
            task {
                use req = new HttpRequestMessage(HttpMethod.Post, url)
                req.Headers.Authorization <- AuthenticationHeaderValue("Bearer", opts.ApiKey)
                req.Headers.Accept.Add(MediaTypeWithQualityHeaderValue("application/json"))
                req.Content <- new StringContent(body, Encoding.UTF8, "application/json")

                use! resp = http.SendAsync(req)
                let! text = resp.Content.ReadAsStringAsync()

                if resp.IsSuccessStatusCode then
                    return Ok text
                else
                    return Error (resp.StatusCode, text)
            })
    
    // Recurse to attempt a few tries if success response is not returned. This is to try to help
    // with the fact that there could be network. Recurses through to make attempts based on the error returned.
    // Compounding messages if no success is returned, or removing the errrors if the success is made along the way.
    let rec tryModel
        (model: string)
        (errors: string list)
        (rawQuery: string) =
            task {
                if String.IsNullOrWhiteSpace model
                then
                    return {
                        TitleOpt = None
                        AuthorOpt = None
                        Keywords = []
                        YearOpt = None
                        Ambiguity = Some "ChatGpt unavailable"
                    }, List.rev errors
                else
                    match! callChatGptModel opts model rawQuery with
                    | Ok body ->
                        use doc = JsonDocument.Parse body
                        match ExtractionParsing.tryGetChatGptText doc with
                        | Ok ex -> 
                            match ExtractionParsing.parseExtracted ex with
                            | Ok extracted ->
                                return extracted, [ $"ChatGpt model used: {model}" ]
                            | Error e ->
                                return! 
                                    tryModel
                                        model
                                        ($"ChatGpt parse failed ({model}): {e}" :: errors)
                                        rawQuery
                        | Error er ->  
                            return! 
                                tryModel
                                    model
                                    ($"ChatGpt parse failed ({model}): {er}" :: errors)
                                    rawQuery

                    | Error (HttpStatusCode.NotFound, _) ->
                        return! tryModel
                            model
                            ($"ChatGpt model not found ({model})" :: errors)
                            rawQuery

                    | Error (status, body) ->
                        return! tryModel
                            model
                            ($"ChatGpt call failed ({model}): {(int status)} {body}" :: errors)
                            rawQuery
            }

    interface IQueryExtractor with
        member _.Extract(rawQuery: string) =
            task {
                if String.IsNullOrWhiteSpace rawQuery then
                    return { 
                        TitleOpt = None
                        AuthorOpt = None
                        Keywords = []
                        YearOpt = None
                        Ambiguity = Some "No query supplied" 
                    }, [ $"You need to submit some combination of author, title, or keywords " ]
                else
                    return! tryModel opts.Model [] rawQuery
            }
