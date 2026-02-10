namespace LibraryDiscovery.Endpoints.Discovery

open Shared.LibraryDiscovery
open Microsoft.AspNetCore.Http

module LibraryDiscoveryApi =

    let api (ctx: HttpContext) : ILibraryDiscoveryApi =
        // Get the DI service
        let svc = ctx.GetService<IDiscoveryService>()
        {
            discover =
                fun req -> async {
                    let! resp = svc.Discover(req.query) |> Async.AwaitTask
                    return resp
                }
        }
