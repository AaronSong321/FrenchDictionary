// Learn more about F# at http://fsharp.org

open System

open Jmas.FrenchDictionary

[<EntryPoint>]
let main argv =
    match argv with
    | [| exeName; opName; options |] -> printf "%s" opName
    | _ -> ()
    
    0 // return an integer exit code
