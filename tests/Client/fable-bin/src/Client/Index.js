import { Union, Record } from "../../fable_modules/fable-library-js.4.19.3/Types.js";
import { union_type, class_type, record_type, bool_type, option_type, string_type } from "../../fable_modules/fable-library-js.4.19.3/Reflection.js";
import { LibrarySearchRequest, ILibraryDiscoveryApi_$reflection, LibrarySearchResponse_$reflection } from "../Shared/Shared.js";
import { Remoting_buildProxy_64DC51C } from "../../fable_modules/Fable.Remoting.Client.7.34.0/Remoting.fs.js";
import { RemotingModule_createApi, RemotingModule_withRouteBuilder } from "../../fable_modules/Fable.Remoting.Client.7.34.0/Remoting.fs.js";
import { createObj, uncurry2 } from "../../fable_modules/fable-library-js.4.19.3/Util.js";
import { isNullOrWhiteSpace, printf, toText } from "../../fable_modules/fable-library-js.4.19.3/String.js";
import { Cmd_none } from "../../fable_modules/Fable.Elmish.4.3.0/cmd.fs.js";
import { Cmd_OfAsyncWith_either } from "../../fable_modules/Fable.Elmish.4.3.0/cmd.fs.js";
import { AsyncHelpers_start } from "../../fable_modules/Fable.Elmish.4.3.0/prelude.fs.js";
import { createElement } from "react";
import * as react from "react";
import { map, isEmpty, ofArray } from "../../fable_modules/fable-library-js.4.19.3/List.js";
import { reactApi } from "../../fable_modules/Feliz.2.9.0/Interop.fs.js";
import { defaultOf } from "../../fable_modules/fable-library-js.4.19.3/Util.js";
import { singleton, append, delay, toList } from "../../fable_modules/fable-library-js.4.19.3/Seq.js";

export class Model extends Record {
    constructor(Input, Response, Error$, IsLoading) {
        super();
        this.Input = Input;
        this.Response = Response;
        this.Error = Error$;
        this.IsLoading = IsLoading;
    }
}

export function Model_$reflection() {
    return record_type("Index.Model", [], Model, () => [["Input", string_type], ["Response", option_type(LibrarySearchResponse_$reflection())], ["Error", option_type(string_type)], ["IsLoading", bool_type]]);
}

export class Msg extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["SetInput", "Search", "GotResponse", "FailedResponse"];
    }
}

export function Msg_$reflection() {
    return union_type("Index.Msg", [], Msg, () => [[["Item", string_type]], [], [["Item", LibrarySearchResponse_$reflection()]], [["Item", class_type("System.Exception")]]]);
}

export const libraryDiscover = Remoting_buildProxy_64DC51C(RemotingModule_withRouteBuilder(uncurry2((() => {
    const clo = toText(printf("/api/%s/%s"));
    return (arg) => {
        const clo_1 = clo(arg);
        return clo_1;
    };
})()), RemotingModule_createApi()), ILibraryDiscoveryApi_$reflection());

export function init() {
    return [new Model("", undefined, undefined, false), Cmd_none()];
}

export function update(msg, model) {
    switch (msg.tag) {
        case 1: {
            const q = model.Input.trim();
            if (isNullOrWhiteSpace(q)) {
                return [new Model(model.Input, model.Response, "Enter a title, author, or keywords.", model.IsLoading), Cmd_none()];
            }
            else {
                return [new Model(model.Input, model.Response, undefined, true), Cmd_OfAsyncWith_either((x) => {
                    AsyncHelpers_start(x);
                }, libraryDiscover.discover, new LibrarySearchRequest(q), (Item) => (new Msg(2, [Item])), (Item_1) => (new Msg(3, [Item_1])))];
            }
        }
        case 2:
            return [new Model(model.Input, msg.fields[0], undefined, false), Cmd_none()];
        case 3:
            return [new Model(model.Input, model.Response, msg.fields[0].message, false), Cmd_none()];
        default:
            return [new Model(msg.fields[0], model.Response, model.Error, model.IsLoading), Cmd_none()];
    }
}

export function View_searchBar(model, dispatch) {
    let elems, value_2, value_12;
    return createElement("div", createObj(ofArray([["className", "flex flex-col sm:flex-row gap-3 mt-4"], (elems = [createElement("input", createObj(ofArray([(value_2 = "w-full rounded-lg border border-white/40 bg-white/60 px-3 py-2 outline-none focus:ring-2 ring-teal-300 text-sm sm:text-base", ["className", value_2]), ["value", model.Input], ["placeholder", "Try: tolkien hobbit illustrated 1937"], ["autoFocus", true], ["onChange", (ev) => {
        dispatch(new Msg(0, [ev.target.value]));
    }], ["onKeyDown", (ev_1) => {
        if (ev_1.key === "Enter") {
            dispatch(new Msg(1, []));
        }
    }]]))), createElement("button", createObj(ofArray([(value_12 = "px-6 py-2 rounded-lg bg-teal-600 text-white font-semibold hover:bg-teal-700 disabled:opacity-40 disabled:cursor-not-allowed", ["className", value_12]), ["disabled", model.IsLoading ? true : isNullOrWhiteSpace(model.Input.trim())], ["onClick", (_arg) => {
        dispatch(new Msg(1, []));
    }], ["children", model.IsLoading ? "Searching..." : "Search"]])))], ["children", reactApi.Children.toArray(Array.from(elems))])])));
}

export function View_messages(resp) {
    let elems;
    if (isEmpty(resp.messages)) {
        return defaultOf();
    }
    else {
        return createElement("ul", createObj(ofArray([["className", "mt-4 text-xs sm:text-sm text-black/80 list-disc pl-5"], (elems = map((m) => createElement("li", {
            children: m,
        }), resp.messages), ["children", reactApi.Children.toArray(Array.from(elems))])])));
    }
}

export function View_errorBanner(err) {
    return createElement("div", {
        className: "mt-4 rounded-lg border border-red-300 bg-red-50 text-red-900 px-3 py-2 text-sm",
        children: err,
    });
}

export function View_candidateCard(c) {
    let elems_2, elems_1;
    return createElement("div", createObj(ofArray([["className", "mt-4 rounded-xl bg-white/70 border border-white/50 p-3 sm:p-4 shadow-sm"], (elems_2 = [createElement("div", createObj(ofArray([["className", "flex gap-3"], (elems_1 = toList(delay(() => {
        let matchValue;
        return append((matchValue = c.coverUrl, (matchValue != null) ? singleton(createElement("img", {
            src: matchValue,
            className: "h-24 w-16 object-cover rounded-md border border-black/10",
        })) : singleton(defaultOf())), delay(() => {
            let elems;
            return singleton(createElement("div", createObj(ofArray([["className", "flex-1"], (elems = toList(delay(() => append(singleton(createElement("div", {
                className: "text-base sm:text-lg font-semibold text-black",
                children: c.workTitle,
            })), delay(() => append(singleton(createElement("div", {
                className: "text-sm text-black/70",
                children: c.author,
            })), delay(() => {
                let matchValue_1;
                return append((matchValue_1 = c.firstPublishYear, (matchValue_1 != null) ? singleton(createElement("div", {
                    className: "text-xs text-black/60 mt-1",
                    children: `First published: ${matchValue_1}`,
                })) : singleton(defaultOf())), delay(() => singleton(createElement("div", {
                    className: "text-sm text-black/80 mt-2",
                    children: c.explanation,
                }))));
            })))))), ["children", reactApi.Children.toArray(Array.from(elems))])]))));
        }));
    })), ["children", reactApi.Children.toArray(Array.from(elems_1))])])))], ["children", reactApi.Children.toArray(Array.from(elems_2))])])));
}

export function View_results(model) {
    let elems;
    const matchValue = model.Response;
    if (matchValue != null) {
        const resp = matchValue;
        return createElement("div", createObj(ofArray([["className", "mt-2"], (elems = toList(delay(() => append(singleton(View_messages(resp)), delay(() => {
            let xs_3;
            return isEmpty(resp.candidates) ? singleton(createElement("div", {
                className: "mt-6 text-black/70",
                children: "No candidates found.",
            })) : singleton((xs_3 = map(View_candidateCard, resp.candidates), react.createElement(react.Fragment, {}, ...xs_3)));
        })))), ["children", reactApi.Children.toArray(Array.from(elems))])])));
    }
    else if (model.IsLoading) {
        return createElement("div", {
            className: "mt-6 text-black/80",
            children: "Searching Open Library...",
        });
    }
    else {
        return createElement("div", {
            className: "mt-6 text-black/70",
            children: "Enter a query to find a likely match.",
        });
    }
}

export function view(model, dispatch) {
    let elems_2, elems_1, value_11, elems;
    return createElement("section", createObj(ofArray([["className", "min-h-screen w-full relative"], (elems_2 = [createElement("div", {
        className: "absolute inset-0 bg-cover bg-center bg-fixed bg-no-repeat",
        style: {
            backgroundImage: "url(\'https://unsplash.it/1200/900?random\')",
        },
    }), createElement("div", {
        className: "absolute inset-0 bg-white/20 backdrop-blur-sm",
    }), createElement("div", createObj(ofArray([["className", "relative z-10 flex flex-col items-center justify-start pt-10 px-4"], (elems_1 = [createElement("div", createObj(ofArray([(value_11 = "w-full max-w-2xl rounded-2xl bg-white/25 backdrop-blur-lg border border-white/30 shadow-lg p-4 sm:p-8", ["className", value_11]), (elems = toList(delay(() => append(singleton(createElement("h1", {
        className: "text-center text-3xl sm:text-5xl font-bold text-black mb-2",
        children: "InfoTrack_OpenLibrary",
    })), delay(() => {
        let value_19;
        return append(singleton(createElement("p", createObj(ofArray([["className", "text-center text-black/70 text-sm sm:text-base"], (value_19 = "Paste a messy query including any of the following: title || author || keywords; and we\'ll return the most likely book.", ["children", value_19])])))), delay(() => append(singleton(View_searchBar(model, dispatch)), delay(() => {
            let matchValue;
            return append((matchValue = model.Error, (matchValue != null) ? singleton(View_errorBanner(matchValue)) : singleton(defaultOf())), delay(() => singleton(View_results(model))));
        }))));
    })))), ["children", reactApi.Children.toArray(Array.from(elems))])])))], ["children", reactApi.Children.toArray(Array.from(elems_1))])])))], ["children", reactApi.Children.toArray(Array.from(elems_2))])])));
}

