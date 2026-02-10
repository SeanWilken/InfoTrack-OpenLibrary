module Shared.Tests

open Shared
open Shared.LibraryDiscovery

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif


let shared =
    testList "Shared" [

        testCase "LibrarySearchRequest holds query" <| fun _ ->
            let req = { query = "tolkien hobbit" }
            Expect.equal req.query "tolkien hobbit" "query should roundtrip"

        testCase "ExtractedQueryDetailDto accepts null-ish fields" <| fun _ ->
            let dto =
                { titleOpt=None; authorOpt=None; keywords=[]; yearOpt=None; ambiguity=None }
            Expect.equal dto.keywords [] "keywords empty ok"

        testCase "CandidateDto basic construction" <| fun _ ->
            let c =
                { 
                    workTitle="The Hobbit"
                    workId="/works/OL123W"
                    author="J.R.R. Tolkien"
                    authorId="/authors/OL1A"
                    firstPublishYear=Some 1937
                    coverUrl=None
                    explanation="Exact title match."
                }
            Expect.stringContains c.explanation "Exact" "explanation set"

        testCase "LibrarySearchResponse includes messages" <| fun _ ->
            let resp =
                { 
                    normalizedQuery = { titleOpt=None; authorOpt=None; keywords=[]; yearOpt=None; ambiguity=None }
                    candidates = []
                    messages = ["AI provider: Gemini"]
                }
            Expect.equal resp.messages.Length 1 "messages list should exist"

        testCase "CandidateDto supports missing coverUrl" <| fun _ ->
            let c =
                { 
                    workTitle="X"
                    workId="W"
                    author="A"
                    authorId="A1"
                    firstPublishYear=None
                    coverUrl=None
                    explanation="x"
                }
            Expect.isTrue c.coverUrl.IsNone "coverUrl optional"
    ]
