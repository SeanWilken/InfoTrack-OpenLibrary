namespace LibraryDiscovery.Domain

open System

// INTERNAL TYPES FOR SERVER TO WORK WITH
type WorkKey = string
type AuthorId = string

// why do we need this type as well when we have the shared one?
type ExtractedQuery = {
    TitleOpt:           string option
    AuthorOpt:          string option
    Keywords:           string list
    YearOpt:            int option
    Ambiguity:          string option
}

// 
type ResolvedWork = {
    WorkKey        : string
    Title          : string
    Subtitle       : string option
    FirstYear      : int option
    PrimaryAuthors : (AuthorId * string) list // use a dict or map?
    Contributors   : string list // returned lower-signal from results
    CoverUrl       : string option
}

type CandidateSignals = {
    TitleExact:         bool
    TitleNear:          bool
    PrimaryAuthorMatch: bool
    ContributorMatch:   bool
    YearMatch:          bool
}

type Candidate = {
    WorkKey:            WorkKey
    Title:              string
    PrimaryAuthors:     string list
    FirstYear:          int option
    CoverUrl:           string option
    Signals:            CandidateSignals
    Explanation:        string
}


// THESE ARE FOR WORKING WITH OPEN LIBRARY API
module OpenLibrary =

    module Dto =

        // /search.json
        type SearchDoc = {
            key               : string
            title             : string
            author_name       : string[] // contributors can appear here
            first_publish_year: int option
            cover_i           : int option
        }

        type SearchResponse = {
            numFound: int
            docs    : SearchDoc[]
        }

        // /works/{id}.json
        type WorkAuthorRef = {
            author: {| key: string |}
        }

        type WorkDetail = {
            key               : string
            title             : string
            subtitle          : string option
            first_publish_date: string option
            authors           : WorkAuthorRef[] // canonical authors
        }

        // /authors/{id}.json
        type AuthorDetail = {
            key  : string
            name : string
        }
