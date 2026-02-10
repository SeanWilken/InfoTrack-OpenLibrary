module Index

open Elmish
open SAFE
open Shared
open Feliz

type Model = {
    Input: string
    Response: LibraryDiscovery.LibrarySearchResponse option
    Error: string option
    IsLoading: bool
}

type Msg =
    | SetInput of string
    | Search
    | GotResponse of LibraryDiscovery.LibrarySearchResponse
    | FailedResponse of exn

let libraryDiscover = Api.makeProxy<LibraryDiscovery.ILibraryDiscoveryApi> ()

let init () =
    { Input = ""
      Response = None
      Error = None
      IsLoading = false }, Cmd.none

let update msg model =
    match msg with
    | SetInput value ->
        { model with Input = value }, Cmd.none

    | Search ->
        let q = model.Input.Trim()
        if System.String.IsNullOrWhiteSpace q then
            { model with Error = Some "Enter a title, author, or keywords." }, Cmd.none
        else
            let req: LibraryDiscovery.LibrarySearchRequest = { query = q }
            { model with IsLoading = true; Error = None }, // clear error on new search
            Cmd.OfAsync.either
                libraryDiscover.discover
                    req
                    GotResponse
                    FailedResponse

    | GotResponse resp ->
        { model with Response = Some resp; IsLoading = false; Error = None }, Cmd.none

    | FailedResponse ex ->
        { model with Error = Some ex.Message; IsLoading = false }, Cmd.none

module View =

    let searchBar (model: Model) (dispatch: Msg -> unit) =
        Html.div [
            prop.className "flex flex-col sm:flex-row gap-3 mt-4"
            prop.children [
                Html.input [
                    prop.className "w-full rounded-lg border border-white/40 bg-white/60 px-3 py-2 outline-none focus:ring-2 ring-teal-300 text-sm sm:text-base"
                    prop.value model.Input
                    prop.placeholder "Try: tolkien hobbit illustrated 1937"
                    prop.autoFocus true
                    prop.onChange (SetInput >> dispatch)
                    prop.onKeyDown (fun ev ->
                        if ev.key = "Enter" then dispatch Search
                    )
                ]
                Html.button [
                    prop.className "px-6 py-2 rounded-lg bg-teal-600 text-white font-semibold hover:bg-teal-700 disabled:opacity-40 disabled:cursor-not-allowed"
                    prop.disabled (model.IsLoading || System.String.IsNullOrWhiteSpace(model.Input.Trim()))
                    prop.onClick (fun _ -> dispatch Search)
                    prop.text (if model.IsLoading then "Searching..." else "Search")
                ]
            ]
        ]

    let messages (resp: LibraryDiscovery.LibrarySearchResponse) =
        if resp.messages.IsEmpty then Html.none
        else
            Html.ul [
                prop.className "mt-4 text-xs sm:text-sm text-black/80 list-disc pl-5"
                prop.children (
                    resp.messages |> List.map (fun m -> Html.li [ prop.text m ])
                )
            ]

    let errorBanner (err: string) =
        Html.div [
            prop.className "mt-4 rounded-lg border border-red-300 bg-red-50 text-red-900 px-3 py-2 text-sm"
            prop.text err
        ]

    let candidateCard (c: LibraryDiscovery.CandidateDto) =
        Html.div [
            prop.className "mt-4 rounded-xl bg-white/70 border border-white/50 p-3 sm:p-4 shadow-sm"
            prop.children [
                Html.div [
                    prop.className "flex gap-3"
                    prop.children [
                        match c.coverUrl with
                        | None -> Html.none
                        | Some url ->
                            Html.img [
                                prop.src url
                                prop.className "h-24 w-16 object-cover rounded-md border border-black/10"
                            ]
                        Html.div [
                            prop.className "flex-1"
                            prop.children [
                                Html.div [
                                    prop.className "text-base sm:text-lg font-semibold text-black"
                                    prop.text c.workTitle
                                ]
                                Html.div [
                                    prop.className "text-sm text-black/70"
                                    prop.text c.author
                                ]
                                match c.firstPublishYear with
                                | None -> Html.none
                                | Some y ->
                                    Html.div [
                                        prop.className "text-xs text-black/60 mt-1"
                                        prop.text $"First published: {y}"
                                    ]
                                Html.div [
                                    prop.className "text-sm text-black/80 mt-2"
                                    prop.text c.explanation
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    let results (model: Model) =
        match model.Response with
        | None when model.IsLoading ->
            Html.div [ prop.className "mt-6 text-black/80"; prop.text "Searching Open Library..." ]
        | None ->
            Html.div [ prop.className "mt-6 text-black/70"; prop.text "Enter a query to find a likely match." ]
        | Some resp ->
            Html.div [
                prop.className "mt-2"
                prop.children [
                    messages resp
                    if resp.candidates.IsEmpty then
                        Html.div [ prop.className "mt-6 text-black/70"; prop.text "No candidates found." ]
                    else
                        resp.candidates |> List.map candidateCard |> React.fragment
                ]
            ]

let view model dispatch =
    Html.section [
        prop.className "min-h-screen w-full relative"
        prop.children [

            Html.div [
                prop.className "absolute inset-0 bg-cover bg-center bg-fixed bg-no-repeat"
                prop.style [ style.backgroundImageUrl "https://unsplash.it/1200/900?random" ]
            ]
            Html.div [
                prop.className "absolute inset-0 bg-white/20 backdrop-blur-sm"
            ]

            Html.div [
                prop.className "relative z-10 flex flex-col items-center justify-start pt-10 px-4"
                prop.children [
                    Html.div [
                        prop.className "w-full max-w-2xl rounded-2xl bg-white/25 backdrop-blur-lg border border-white/30 shadow-lg p-4 sm:p-8"
                        prop.children [
                            Html.h1 [
                                prop.className "text-center text-3xl sm:text-5xl font-bold text-black mb-2"
                                prop.text "InfoTrack_OpenLibrary"
                            ]
                            Html.p [
                                prop.className "text-center text-black/70 text-sm sm:text-base"
                                prop.text "Paste a messy query including any of the following: title || author || keywords; and we'll return the most likely book."
                            ]

                            View.searchBar model dispatch

                            match model.Error with
                            | None -> Html.none
                            | Some err -> View.errorBanner err

                            View.results model
                        ]
                    ]
                ]
            ]
        ]
    ]
