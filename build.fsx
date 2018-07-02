#r "paket: groupref Build //"
#load "./.fake/build.fsx/intellisense.fsx"
#if !FAKE
  #r "netstandard"
#endif

#load "MonoGameContent.fsx"

open Fake.Core
open Fake.IO.Globbing.Operators

// Directories

let intermediateContentDir = "./intermediateContent/"
let contentDir = "./src/GeoVisualise/"
let buildDir  = "./build/"
let deployDir = "./deploy/"

// Filesets

let appReferences = 
    !! "**/*.fsproj"

let contentFiles =
    !! "**/*.fx"
        ++ "**/*.spritefont"
        ++ "**/*.dds"

// Targets

Target.description "Cleaning directories"
Target.create "Clean" (fun _ -> 
    Fake.IO.Shell.cleanDirs [buildDir; deployDir]
)

Target.description "Building MonoGame content"
Target.create "BuildContent" (fun _ ->
    contentFiles
        |> MonoGameContent.buildMonoGameContent (fun p ->
            { p with
                OutputDir = contentDir;
                IntermediateDir = intermediateContentDir;
            }))

Target.description "Building application"
Target.create "BuildApp" (fun _ ->
    appReferences
        |> Fake.DotNet.MSBuild.runDebug id buildDir "Build"
        |> ignore
)

Target.description "Running application"
Target.create "RunApp" (fun _ ->
    Fake.Core.Process.fireAndForget (fun info ->
        { info with
            FileName = buildDir + @"GeoVisualise.exe"
            WorkingDirectory = buildDir })
    Fake.Core.Process.setKillCreatedProcesses false)

// Build order

open Fake.Core.TargetOperators

"Clean"
    ==> "BuildContent"
    ==> "BuildApp"
    ==> "RunApp"

// Start build

Target.runOrDefault "BuildApp"