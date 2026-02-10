import { Record } from "../../fable_modules/fable-library-js.4.19.3/Types.js";
import { lambda_type, class_type, int32_type, list_type, option_type, record_type, string_type } from "../../fable_modules/fable-library-js.4.19.3/Reflection.js";

export class LibrarySearchRequest extends Record {
    constructor(query) {
        super();
        this.query = query;
    }
}

export function LibrarySearchRequest_$reflection() {
    return record_type("Shared.LibraryDiscovery.LibrarySearchRequest", [], LibrarySearchRequest, () => [["query", string_type]]);
}

export class ExtractedQueryDetailDto extends Record {
    constructor(titleOpt, authorOpt, keywords, yearOpt, ambiguity) {
        super();
        this.titleOpt = titleOpt;
        this.authorOpt = authorOpt;
        this.keywords = keywords;
        this.yearOpt = yearOpt;
        this.ambiguity = ambiguity;
    }
}

export function ExtractedQueryDetailDto_$reflection() {
    return record_type("Shared.LibraryDiscovery.ExtractedQueryDetailDto", [], ExtractedQueryDetailDto, () => [["titleOpt", option_type(string_type)], ["authorOpt", option_type(string_type)], ["keywords", list_type(string_type)], ["yearOpt", option_type(int32_type)], ["ambiguity", option_type(string_type)]]);
}

export class CandidateDto extends Record {
    constructor(workTitle, workId, author, authorId, firstPublishYear, coverUrl, explanation) {
        super();
        this.workTitle = workTitle;
        this.workId = workId;
        this.author = author;
        this.authorId = authorId;
        this.firstPublishYear = firstPublishYear;
        this.coverUrl = coverUrl;
        this.explanation = explanation;
    }
}

export function CandidateDto_$reflection() {
    return record_type("Shared.LibraryDiscovery.CandidateDto", [], CandidateDto, () => [["workTitle", string_type], ["workId", string_type], ["author", string_type], ["authorId", string_type], ["firstPublishYear", option_type(int32_type)], ["coverUrl", option_type(string_type)], ["explanation", string_type]]);
}

export class LibrarySearchResponse extends Record {
    constructor(normalizedQuery, candidates, messages) {
        super();
        this.normalizedQuery = normalizedQuery;
        this.candidates = candidates;
        this.messages = messages;
    }
}

export function LibrarySearchResponse_$reflection() {
    return record_type("Shared.LibraryDiscovery.LibrarySearchResponse", [], LibrarySearchResponse, () => [["normalizedQuery", ExtractedQueryDetailDto_$reflection()], ["candidates", list_type(CandidateDto_$reflection())], ["messages", list_type(string_type)]]);
}

export class ILibraryDiscoveryApi extends Record {
    constructor(discover) {
        super();
        this.discover = discover;
    }
}

export function ILibraryDiscoveryApi_$reflection() {
    return record_type("Shared.LibraryDiscovery.ILibraryDiscoveryApi", [], ILibraryDiscoveryApi, () => [["discover", lambda_type(LibrarySearchRequest_$reflection(), class_type("Microsoft.FSharp.Control.FSharpAsync`1", [LibrarySearchResponse_$reflection()]))]]);
}

