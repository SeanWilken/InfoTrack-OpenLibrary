module Server.Tests

open Expecto

open Shared
open LibraryDiscovery.AI.ExtractionParsing
open LibraryDiscovery.Domain

open LibraryDiscovery.Utils.Normalize

[<Tests>]
let tests =
    testList "Normalize" [

        test "normalizeText removes punctuation + collapses whitespace" {
            let x = normalizeText " The  Hobbit!!  (1937) "
            Expect.equal x "the hobbit 1937" "should normalize"
        }

        test "normalizeText removes diacritics" {
            let x = normalizeText "García Márquez"
            Expect.equal x "garcia marquez" "should remove diacritics"
        }

        test "mainTitle splits subtitles" {
            let x = mainTitle "The Hobbit: There and Back Again"
            Expect.equal x "the hobbit" "main title should be left side"
        }

        test "jaccard works for overlap" {
            let a = tokens "tolkien hobbit illustrated"
            let b = tokens "hobbit deluxe illustrated"
            let j = jaccard a b
            Expect.isGreaterThan j 0.3 "should overlap meaningfully"
        }

        test "tryExtractYear finds 1937" {
            let y = tryExtractYear "tolkein hobbit illustrated deluxe 1937"
            Expect.equal y (Some 1937) "should extract year"
        }
    ]



module Fake =
    let rw (title: string) (subtitle: string option) (authors: (string * string) list) (contributors: string list) (year: int option) =
        {
            WorkKey = "/works/OL123W"
            Title = title
            Subtitle = subtitle
            PrimaryAuthors = authors // e.g. [("key","J.R.R. Tolkien")]
            Contributors = contributors
            FirstYear = year
            CoverUrl = None
        }

    let ex (titleOpt: string option) (authorOpt: string option) (keywords: string list) (yearOpt: int option) =
        {
            TitleOpt = titleOpt
            AuthorOpt = authorOpt
            Keywords = keywords
            YearOpt = yearOpt
            Ambiguity = None
        }

[<Tests>]
let aiTests =
    testList "AI Parsing + Explain" [

        test "parseExtracted parses title + keyword" {
            let json = """{ "titleOpt":"The hobbit", "authorOpt":null, "keywords":["illustrated"], "yearOpt":null, "ambiguity":null }"""
            match parseExtracted json with
            | Ok ex ->
                Expect.equal ex.TitleOpt (Some "The hobbit") "titleOpt"
                Expect.equal ex.Keywords ["illustrated"] "keywords"
            | Error e ->
                failwith e
        }

        test "parseExtracted parses author + year" {
            let json = """{ "titleOpt":null, "authorOpt":"Donn Pearce", "keywords":[], "yearOpt":1966, "ambiguity":null }"""
            match parseExtracted json with
            | Ok ex ->
                Expect.equal ex.AuthorOpt (Some "Donn Pearce") "authorOpt"
                Expect.equal ex.YearOpt (Some 1966) "yearOpt"
            | Error e ->
                failwith e
        }

        test "mkExplanation prefers primary author over contributor" {
            // You may need to expose mkExplanation or move to a public module to test.
            let extracted = Fake.ex (Some "Cool Hand Luke") (Some "Donn Pearce") [] (Some 1966)

            let rw =
                Fake.rw
                    "Cool Hand Luke"
                    None
                    [ ("/authors/OL1A", "Donn Pearce") ]
                    [ "Some Illustrator"; "Donn Pearce" ] // contributor list may contain name too
                    (Some 1966)

            let signals = {
                TitleExact = true
                TitleNear = false
                PrimaryAuthorMatch = true
                ContributorMatch = true
                YearMatch = true
            }

            let msg = LibraryDiscovery.Application.Discovery.mkExplanation extracted rw signals
            Expect.stringContains msg "primary author" "should mention primary author"
            Expect.isFalse (msg.Contains("contributor (lower signal)") && signals.PrimaryAuthorMatch) "should not lead with contributor when primary matches"
        }

        test "mkExplanation includes year match when provided" {
            let extracted = Fake.ex (Some "The Hobbit") None [] (Some 1937)
            let rw = Fake.rw "The Hobbit" None [("/authors/OL1A", "J.R.R. Tolkien")] [] (Some 1937)

            let signals = {
                TitleExact = true
                TitleNear = false
                PrimaryAuthorMatch = false
                ContributorMatch = false
                YearMatch = true
            }

            let msg = LibraryDiscovery.Application.Discovery.mkExplanation extracted rw signals
            Expect.stringContains msg "1937" "should mention year"
        }

        test "mkExplanation falls back to Open Library relevance when no signals" {
            let extracted = Fake.ex None None [] None
            let rw = Fake.rw "Something" None [] [] None
            let signals = { TitleExact=false; TitleNear=false; PrimaryAuthorMatch=false; ContributorMatch=false; YearMatch=false }

            let msg = LibraryDiscovery.Application.Discovery.mkExplanation extracted rw signals
            Expect.stringContains msg "Open Library" "should mention relevance"
        }
    ]


let all = testList "All" [ Shared.Tests.shared;  tests; aiTests;  ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all