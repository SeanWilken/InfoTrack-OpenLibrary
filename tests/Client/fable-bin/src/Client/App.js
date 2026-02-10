import { ProgramModule_mkProgram, ProgramModule_run } from "../../fable_modules/Fable.Elmish.4.3.0/program.fs.js";
import { Program_withReactSynchronous } from "../../fable_modules/Fable.Elmish.React.4.0.0/react.fs.js";
import { view, update, init } from "./Index.js";
import "../../../../../src/Client/index.css";


ProgramModule_run(Program_withReactSynchronous("elmish-app", ProgramModule_mkProgram(init, update, view)));

