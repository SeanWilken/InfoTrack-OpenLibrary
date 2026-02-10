namespace Shared

open System

module LibraryDiscovery =

    [<CLIMutable>]
    type LibrarySearchRequest = { query: string }

    [<CLIMutable>]
    type ExtractedQueryDetailDto = {
        titleOpt: string option
        authorOpt: string option
        keywords: string list
        yearOpt: int option
        ambiguity: string option
    }

    [<CLIMutable>]
    type CandidateDto = {
        workTitle: string
        workId: string
        author: string
        authorId: string
        firstPublishYear: int option
        coverUrl: string option
        explanation: string
    }

    [<CLIMutable>]
    type LibrarySearchResponse = {
        normalizedQuery: ExtractedQueryDetailDto
        candidates: CandidateDto list
        messages: string list
    }

    type ILibraryDiscoveryApi = {
        discover : LibrarySearchRequest -> Async<LibrarySearchResponse>
    }
