// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------

(*
    This file handles the complete build process of RazorEngine

    The first step is handled in build.sh and build.cmd by bootstrapping a NuGet.exe and 
    executing NuGet to resolve all build dependencies (dependencies required for the build to work, for example FAKE)

    The secound step is executing this file which resolves all dependencies, builds the solution and executes all unit tests
*)

#if FAKE
#else
// Support when file is opened in Visual Studio
#load "packages/Yaaf.AdvancedBuilding/content/buildConfigDef.fsx"
#endif

open BuildConfigDef
open System.Collections.Generic
open System.IO

open Fake
open Fake.Git
open Fake.FSharpFormatting
open AssemblyInfoFile

if isMono then
    monoArguments <- "--runtime=v4.0 --debug"

let buildConfig =
 // Read release notes document
 let release = ReleaseNotesHelper.parseReleaseNotes (File.ReadLines "doc/ReleaseNotes.md")
 { BuildConfiguration.Defaults with
    ProjectName = "Yaaf.Sasl.Ldap"
    CopyrightNotice = "Yaaf.Sasl.Ldap Copyright © Matthias Dittrich 2011-2015"
    ProjectSummary = "Yaaf.Sasl.Ldap is a server LDAP backend for Yaaf.Sasl."
    ProjectDescription = "Yaaf.Sasl.Ldap is a server LDAP backend for Yaaf.Sasl."
    ProjectAuthors = ["Matthias Dittrich"]
    NugetTags =  "sasl C# F# dotnet .net ldap"
    PageAuthor = "Matthias Dittrich"
    GithubUser = "matthid"
    Version = release.NugetVersion
    NugetPackages =
      [ "Yaaf.Sasl.Ldap.nuspec", (fun config p ->
          { p with
              ReleaseNotes = toLines release.Notes
              Dependencies = 
                [ "FSharp.Core"
                  "Yaaf.Sasl"
                  "Mono.Security"
                  "Novell.Directory.Ldap"
                  "Yaaf.FSharp.Helper" ]
                  |> List.map (fun name -> name, (GetPackageVersion "packages" name)) }) ]
    UseNuget = false
    SetAssemblyFileVersions = (fun config ->
      let info =
        [ Attribute.Company config.ProjectName
          Attribute.Product config.ProjectName
          Attribute.Copyright config.CopyrightNotice
          Attribute.Version config.Version
          Attribute.FileVersion config.Version
          Attribute.InformationalVersion config.Version]
      CreateFSharpAssemblyInfo "./src/SharedAssemblyInfo.fs" info)
    EnableProjectFileCreation = false
    BuildTargets =
     [ { BuildParams.WithSolution with
          // The default build
          PlatformName = "Net40"
          SimpleBuildName = "net40" }
       { BuildParams.WithSolution with
          // The generated templates
          PlatformName = "Net45"
          SimpleBuildName = "net45" } ]
  }
