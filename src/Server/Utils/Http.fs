namespace LibraryDiscovery.Utils

open System.Threading.Tasks
open System.Net

module Http =

    // Allows for attempting retries for http endpoints based on their response. Came from
    // hitting quota limits, not found endpoints for certain models, overwhelmed endpoints...
    let rec retryAsync
        (attemptsLeft: int)
        (delayMs: int)
        (work: unit -> Task<Result<'a, HttpStatusCode * string>>) =
        task {
            let! result = work()
            match result with
            | Ok _ -> return result
            | Error (status, _) when
                attemptsLeft > 1 &&
                (status = HttpStatusCode.ServiceUnavailable ||
                status = HttpStatusCode.TooManyRequests) ->
                do! Task.Delay(delayMs)
                return! retryAsync (attemptsLeft - 1) (delayMs * 2) work
            | _ ->
                return result
        }