module Client.Tests

open Fable.Mocha

open Index
open Shared
open SAFE

let client =
    testList "Client" [

        testCase "SetInput updates input" <| fun _ ->
            let model, _ = init()
            let model2, _ = update (SetInput "hobbit") model
            Expect.equal model2.Input "hobbit" "input should update"

        testCase "SendLibraryDiscoveryRequest does not clear input" <| fun _ ->
            let model, _ = init()
            let model = { model with Input = "cool hand luke" }
            let model2, _ = update Index.Msg.Search model
            Expect.equal model2.Input "cool hand luke" "input preserved"

        testCase "GotLibraryDiscoveryRequest stores response" <| fun _ ->
            let model, _ = init()
            let resp : Shared.LibraryDiscovery.LibrarySearchResponse =
                { 
                    normalizedQuery =
                        {   
                            titleOpt=None
                            authorOpt=None
                            keywords=[]
                            yearOpt=None
                            ambiguity=None
                        }
                    candidates = []
                    messages = ["ok"]
                }
            let model2, _ = update (Index.Msg.GotResponse resp) model
            Expect.isSome model2.Response "response should be set"

        testCase "FailedLibraryDiscoveryRequest stores error" <| fun _ ->
            let model, _ = init()
            let ex = System.Exception("boom")
            let model2, _ = update (FailedResponse ex) model
            Expect.isSome model2.Error "error should be set"

        testCase "GotLibraryDiscoveryRequest clears previous error" <| fun _ ->
            let model, _ = init()
            let model = { model with Error = Some "old" }
            let resp : Shared.LibraryDiscovery.LibrarySearchResponse =
                { 
                    normalizedQuery =
                        {
                            titleOpt = None
                            authorOpt = None
                            keywords = []
                            yearOpt = None
                            ambiguity = None
                        }
                    candidates = []
                    messages = ["ok"]
                }
            let model2, _ = update (GotResponse resp) model
            // If you haven't implemented this behavior yet, decide now:
            // I recommend clearing error on success.
            // If you want that, update your code accordingly and keep this test.
            Expect.isNone model2.Error "error should clear on success"
    ]


[<EntryPoint>]
let main _ = Mocha.runTests client