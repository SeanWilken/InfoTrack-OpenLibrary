import { Mocha_runTests, Expect_isNone, Expect_isSome, Test_testCase, Test_testList } from "./fable_modules/Fable.Mocha.2.17.0/Mocha.fs.js";
import { Model, init, Msg, update } from "./src/Client/Index.js";
import { structuralHash, assertEqual } from "./fable_modules/fable-library-js.4.19.3/Util.js";
import { singleton, empty, ofArray, contains } from "./fable_modules/fable-library-js.4.19.3/List.js";
import { equals, class_type, decimal_type, float64_type, bool_type, int32_type, string_type } from "./fable_modules/fable-library-js.4.19.3/Reflection.js";
import { printf, toText } from "./fable_modules/fable-library-js.4.19.3/String.js";
import { LibrarySearchResponse, ExtractedQueryDetailDto } from "./src/Shared/Shared.js";

export const client = Test_testList("Client", ofArray([Test_testCase("SetInput updates input", () => {
    let copyOfStruct;
    const actual = update(new Msg(0, ["hobbit"]), init()[0])[0].Input;
    if ((actual === "hobbit") ? true : !(new Function("try {return this===window;}catch(e){ return false;}"))()) {
        assertEqual(actual, "hobbit", "input should update");
    }
    else {
        throw new Error(contains((copyOfStruct = actual, string_type), ofArray([int32_type, bool_type, float64_type, string_type, decimal_type, class_type("System.Guid")]), {
            Equals: equals,
            GetHashCode: structuralHash,
        }) ? toText(printf("<span style=\'color:black\'>Expected:</span> <br /><div style=\'margin-left:20px; color:crimson\'>%s</div><br /><span style=\'color:black\'>Actual:</span> </br ><div style=\'margin-left:20px;color:crimson\'>%s</div><br /><span style=\'color:black\'>Message:</span> </br ><div style=\'margin-left:20px; color:crimson\'>%s</div>"))("hobbit")(actual)("input should update") : toText(printf("<span style=\'color:black\'>Expected:</span> <br /><div style=\'margin-left:20px; color:crimson\'>%A</div><br /><span style=\'color:black\'>Actual:</span> </br ><div style=\'margin-left:20px;color:crimson\'>%A</div><br /><span style=\'color:black\'>Message:</span> </br ><div style=\'margin-left:20px; color:crimson\'>%s</div>"))("hobbit")(actual)("input should update"));
    }
}), Test_testCase("SendLibraryDiscoveryRequest does not clear input", () => {
    let copyOfStruct_1;
    const model_1 = init()[0];
    const actual_1 = update(new Msg(1, []), new Model("cool hand luke", model_1.Response, model_1.Error, model_1.IsLoading))[0].Input;
    if ((actual_1 === "cool hand luke") ? true : !(new Function("try {return this===window;}catch(e){ return false;}"))()) {
        assertEqual(actual_1, "cool hand luke", "input preserved");
    }
    else {
        throw new Error(contains((copyOfStruct_1 = actual_1, string_type), ofArray([int32_type, bool_type, float64_type, string_type, decimal_type, class_type("System.Guid")]), {
            Equals: equals,
            GetHashCode: structuralHash,
        }) ? toText(printf("<span style=\'color:black\'>Expected:</span> <br /><div style=\'margin-left:20px; color:crimson\'>%s</div><br /><span style=\'color:black\'>Actual:</span> </br ><div style=\'margin-left:20px;color:crimson\'>%s</div><br /><span style=\'color:black\'>Message:</span> </br ><div style=\'margin-left:20px; color:crimson\'>%s</div>"))("cool hand luke")(actual_1)("input preserved") : toText(printf("<span style=\'color:black\'>Expected:</span> <br /><div style=\'margin-left:20px; color:crimson\'>%A</div><br /><span style=\'color:black\'>Actual:</span> </br ><div style=\'margin-left:20px;color:crimson\'>%A</div><br /><span style=\'color:black\'>Message:</span> </br ><div style=\'margin-left:20px; color:crimson\'>%s</div>"))("cool hand luke")(actual_1)("input preserved"));
    }
}), Test_testCase("GotLibraryDiscoveryRequest stores response", () => {
    Expect_isSome(update(new Msg(2, [new LibrarySearchResponse(new ExtractedQueryDetailDto(undefined, undefined, empty(), undefined, undefined), empty(), singleton("ok"))]), init()[0])[0].Response, "response should be set");
}), Test_testCase("FailedLibraryDiscoveryRequest stores error", () => {
    const patternInput_6 = init();
    Expect_isSome(update(new Msg(3, [new Error("boom")]), patternInput_6[0])[0].Error, "error should be set");
}), Test_testCase("GotLibraryDiscoveryRequest clears previous error", () => {
    const model_5 = init()[0];
    Expect_isNone(update(new Msg(2, [new LibrarySearchResponse(new ExtractedQueryDetailDto(undefined, undefined, empty(), undefined, undefined), empty(), singleton("ok"))]), new Model(model_5.Input, model_5.Response, "old", model_5.IsLoading))[0].Error, "error should clear on success");
})]));

(function (_arg) {
    return Mocha_runTests(client);
})(typeof process === 'object' ? process.argv.slice(2) : []);

