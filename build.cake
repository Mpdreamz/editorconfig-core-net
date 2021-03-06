#load nuget:StarkBIM?package=StarkBIM.Cake.Recipe&version=0.2.71-gcf6587c3ea&prerelease=true

//////////////////////////////////////////////////////////////////////
// VARIABLES
//////////////////////////////////////////////////////////////////////

const string Project = "EditorConfig";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

// Run this before configuring projects
// Certain properties like IsDebug must be set first
BuildParameters.Initialize(Context, BuildSystem, Project, directorySettings: new DirectorySettings(Context, testDirectory: "./src"));

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

// Add any project-specific targets here

BuildParameters.ConfigureProjects(unitTestFilter: @"\**\*.Tests.csproj");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////
Build.Run();