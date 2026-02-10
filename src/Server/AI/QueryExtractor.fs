namespace LibraryDiscovery.AI

open System.Threading.Tasks
open LibraryDiscovery.Domain

module QueryExtractor =

    /// C# friendly :)
    type IQueryExtractor =
        abstract Extract : rawQuery:string -> Task<ExtractedQuery * string list>
