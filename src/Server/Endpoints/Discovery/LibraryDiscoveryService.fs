namespace LibraryDiscovery.Endpoints.Discovery

open System.Threading.Tasks
open Shared.LibraryDiscovery

// Interface for the endpoint
type IDiscoveryService =
    abstract Discover : query:string -> Task<LibrarySearchResponse>
