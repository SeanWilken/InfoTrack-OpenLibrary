namespace LibraryDiscovery.Utils

open System.Text.Json


// Single place for using JSON helpers. Centralized for consistency 
module JsonExtractor =

    /// Try get a property by name
    let tryProp (name: string) (el: JsonElement) =
        match el.TryGetProperty(name) with
        | true, elProp  -> Some elProp
        | false, _ -> None

    /// Try index into array
    let tryIndex (i: int) (el: JsonElement) =
        if el.ValueKind = JsonValueKind.Array && el.GetArrayLength() > i then
            Some (el.[i])
        else None

    /// Try get string value
    let tryString (el: JsonElement) =
        if el.ValueKind = JsonValueKind.String then
            el.GetString() |> Option.ofObj
        else None

    /// Try get int value
    let tryInt (el: JsonElement) =
        if el.ValueKind = JsonValueKind.Number then
            match el.TryGetInt32() with
            | true, i -> Some i
            | _ -> None
        else None

    /// Traverse a property path safely
    /// Example: tryPath ["choices"; "0"; "message"; "content"]
    let tryPath (path: string list) (root: JsonElement) =
        let rec loop el remaining =
            match remaining with
            | [] -> Some el
            | h :: t ->
                match h with
                | (idx: string) when idx |> System.Int32.TryParse |> (fun (f,s) -> f) ->
                    let i = int idx
                    tryIndex i el |> Option.bind (fun e -> loop e t)
                | prop ->
                    tryProp prop el |> Option.bind (fun e -> loop e t)
        loop root path

    /// Convenience: extract string at path
    let tryStringAt path root =
        tryPath path root |> Option.bind tryString
