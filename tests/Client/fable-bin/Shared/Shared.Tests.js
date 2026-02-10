import { Expect_isTrue, Expect_stringContains, Test_testCase, Test_testList } from "../fable_modules/Fable.Mocha.2.17.0/Mocha.fs.js";
import { LibrarySearchResponse, CandidateDto, ExtractedQueryDetailDto, LibrarySearchRequest } from "../src/Shared/Shared.js";
import { int32ToString, equals as equals_1, structuralHash, assertEqual } from "../fable_modules/fable-library-js.4.19.3/Util.js";
import { singleton, length, empty, ofArray, contains } from "../fable_modules/fable-library-js.4.19.3/List.js";
import { list_type, equals, class_type, decimal_type, float64_type, bool_type, int32_type, string_type } from "../fable_modules/fable-library-js.4.19.3/Reflection.js";
import { printf, toText } from "../fable_modules/fable-library-js.4.19.3/String.js";
import { seqToString } from "../fable_modules/fable-library-js.4.19.3/Types.js";

export const shared = Test_testList("Shared", ofArray([Test_testCase("LibrarySearchRequest holds query", () => {
    let copyOfStruct;
    const actual = (new LibrarySearchRequest("tolkien hobbit")).query;
    if ((actual === "tolkien hobbit") ? true : !(new Function("try {return this===window;}catch(e){ return false;}"))()) {
        assertEqual(actual, "tolkien hobbit", "query should roundtrip");
    }
    else {
        throw new Error(contains((copyOfStruct = actual, string_type), ofArray([int32_type, bool_type, float64_type, string_type, decimal_type, class_type("System.Guid")]), {
            Equals: equals,
            GetHashCode: structuralHash,
        }) ? toText(printf("<span style=\'color:black\'>Expected:</span> <br /><div style=\'margin-left:20px; color:crimson\'>%s</div><br /><span style=\'color:black\'>Actual:</span> </br ><div style=\'margin-left:20px;color:crimson\'>%s</div><br /><span style=\'color:black\'>Message:</span> </br ><div style=\'margin-left:20px; color:crimson\'>%s</div>"))("tolkien hobbit")(actual)("query should roundtrip") : toText(printf("<span style=\'color:black\'>Expected:</span> <br /><div style=\'margin-left:20px; color:crimson\'>%A</div><br /><span style=\'color:black\'>Actual:</span> </br ><div style=\'margin-left:20px;color:crimson\'>%A</div><br /><span style=\'color:black\'>Message:</span> </br ><div style=\'margin-left:20px; color:crimson\'>%s</div>"))("tolkien hobbit")(actual)("query should roundtrip"));
    }
}), Test_testCase("ExtractedQueryDetailDto accepts null-ish fields", () => {
    let copyOfStruct_1, arg_6, arg_1_1;
    const actual_1 = (new ExtractedQueryDetailDto(undefined, undefined, empty(), undefined, undefined)).keywords;
    const expected_1 = empty();
    if (equals_1(actual_1, expected_1) ? true : !(new Function("try {return this===window;}catch(e){ return false;}"))()) {
        assertEqual(actual_1, expected_1, "keywords empty ok");
    }
    else {
        throw new Error(contains((copyOfStruct_1 = actual_1, list_type(string_type)), ofArray([int32_type, bool_type, float64_type, string_type, decimal_type, class_type("System.Guid")]), {
            Equals: equals,
            GetHashCode: structuralHash,
        }) ? ((arg_6 = seqToString(expected_1), (arg_1_1 = seqToString(actual_1), toText(printf("<span style=\'color:black\'>Expected:</span> <br /><div style=\'margin-left:20px; color:crimson\'>%s</div><br /><span style=\'color:black\'>Actual:</span> </br ><div style=\'margin-left:20px;color:crimson\'>%s</div><br /><span style=\'color:black\'>Message:</span> </br ><div style=\'margin-left:20px; color:crimson\'>%s</div>"))(arg_6)(arg_1_1)("keywords empty ok")))) : toText(printf("<span style=\'color:black\'>Expected:</span> <br /><div style=\'margin-left:20px; color:crimson\'>%A</div><br /><span style=\'color:black\'>Actual:</span> </br ><div style=\'margin-left:20px;color:crimson\'>%A</div><br /><span style=\'color:black\'>Message:</span> </br ><div style=\'margin-left:20px; color:crimson\'>%s</div>"))(expected_1)(actual_1)("keywords empty ok"));
    }
}), Test_testCase("CandidateDto basic construction", () => {
    Expect_stringContains((new CandidateDto("The Hobbit", "/works/OL123W", "J.R.R. Tolkien", "/authors/OL1A", 1937, undefined, "Exact title match.")).explanation, "Exact", "explanation set");
}), Test_testCase("LibrarySearchResponse includes messages", () => {
    let copyOfStruct_2, arg_7, arg_1_2;
    const actual_2 = length((new LibrarySearchResponse(new ExtractedQueryDetailDto(undefined, undefined, empty(), undefined, undefined), empty(), singleton("AI provider: Gemini"))).messages) | 0;
    if ((actual_2 === 1) ? true : !(new Function("try {return this===window;}catch(e){ return false;}"))()) {
        assertEqual(actual_2, 1, "messages list should exist");
    }
    else {
        throw new Error(contains((copyOfStruct_2 = actual_2, int32_type), ofArray([int32_type, bool_type, float64_type, string_type, decimal_type, class_type("System.Guid")]), {
            Equals: equals,
            GetHashCode: structuralHash,
        }) ? ((arg_7 = int32ToString(1), (arg_1_2 = int32ToString(actual_2), toText(printf("<span style=\'color:black\'>Expected:</span> <br /><div style=\'margin-left:20px; color:crimson\'>%s</div><br /><span style=\'color:black\'>Actual:</span> </br ><div style=\'margin-left:20px;color:crimson\'>%s</div><br /><span style=\'color:black\'>Message:</span> </br ><div style=\'margin-left:20px; color:crimson\'>%s</div>"))(arg_7)(arg_1_2)("messages list should exist")))) : toText(printf("<span style=\'color:black\'>Expected:</span> <br /><div style=\'margin-left:20px; color:crimson\'>%A</div><br /><span style=\'color:black\'>Actual:</span> </br ><div style=\'margin-left:20px;color:crimson\'>%A</div><br /><span style=\'color:black\'>Message:</span> </br ><div style=\'margin-left:20px; color:crimson\'>%s</div>"))(1)(actual_2)("messages list should exist"));
    }
}), Test_testCase("CandidateDto supports missing coverUrl", () => {
    Expect_isTrue((new CandidateDto("X", "W", "A", "A1", undefined, undefined, "x")).coverUrl == null)("coverUrl optional");
})]));

