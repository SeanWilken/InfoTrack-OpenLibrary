namespace LibraryDiscovery.Utils

open System
open System.Globalization
open System.Text

module Normalize =

    // Remove Diacritics
    let private removeDiacritics (input: string) =
        let norm = input.Normalize(NormalizationForm.FormD)
        let sb = StringBuilder(norm.Length)

        norm.ToCharArray()
        |> Array.iter (
            fun char ->
                CharUnicodeInfo.GetUnicodeCategory(char)
                |> fun categoryInfo ->
                    if categoryInfo <> UnicodeCategory.NonSpacingMark 
                    then sb.Append(char) |> ignore
                    else ()
        )
         
        sb.ToString().Normalize(NormalizationForm.FormC)

    // characters to keep
    let private isWordChar (c: char) =
        Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c)

    // normalize text to be sanitized for injecting into LLM as prompt.
    // prompt injection is a concern right now.
    let normalizeText (input: string) =
        if String.IsNullOrWhiteSpace input then "" else input
        |> removeDiacritics
        |> fun s -> s.ToLowerInvariant()
        |> Seq.map ( fun x -> if isWordChar x then x else ' ')
        |> Seq.toArray
        |> String
        |> fun s -> s.Trim()
        |> fun s -> System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ")

    let tokens (input: string) =
        normalizeText input
        |> fun s -> s.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        |> Set.ofArray

    // parse out if there seems to be a ful title
    let mainTitle (title: string) =
        // Early exit if empty
        if String.IsNullOrWhiteSpace title 
        then ""
        else
            // normalize the full title
            let trimmedTitle = title.Trim()
            let separators = [| ":"; " - "; "-"; " _ "; "_"; |]
            
            separators
            |> Array.choose (
                fun sep ->
                    let idx = trimmedTitle.IndexOf(sep, StringComparison.Ordinal)
                    if idx > 0 
                    then Some (trimmedTitle.Substring(0, idx).Trim())
                    else None
            )
            |> Array.tryHead
            |> Option.defaultValue trimmedTitle
            |> normalizeText

    // Check similarity between two sets
    let jaccard (a: Set<string>) (b: Set<string>) =
        if a.IsEmpty && b.IsEmpty then 1.0
        elif a.IsEmpty || b.IsEmpty then 0.0
        else
            let inter = Set.intersect a b |> Set.count |> float
            let uni = Set.union a b |> Set.count |> float
            inter / uni

    // REGEX to try and find a year for the publication
    let tryExtractYear (input: string) =
        System.Text.RegularExpressions.Regex.Match(input, @"\b(1[5-9]\d{2}|20\d{2})\b")
        |> fun x -> 
            if x.Success
            then 
                match Int32.TryParse(x.Groups.[1].Value) with
                | true, i -> Some i
                | false, _ -> None
            else None
