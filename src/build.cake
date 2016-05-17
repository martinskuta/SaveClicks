///////////////////////////////////////////////////////////////////////////////
// ADDINS
///////////////////////////////////////////////////////////////////////////////
#addin "Cake.FileHelpers"

///////////////////////////////////////////////////////////////////////////////
// TOOLS
///////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
// Example command:
// .\build.ps1 --productVersion="1.0"
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = Argument("productVersion", "9.9.9.9");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Restore-NuGet-Packages")
    .Does(() =>
{
	NuGetRestore("./SaveClicks.sln");
});

Task("PatchVersion")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    Information("Version: " + version);


	ReplaceTextInFiles("./AssemblyVersion.cs",
                        "9.9.9.9",
                        version);

	ReplaceTextInFiles("./**/source.extension.vsixmanifest",
						"9.9.9.9",
						version);
});

Task("Build")
    .IsDependentOn("PatchVersion")
    .Does(() =>
{
    MSBuild("./SaveClicks.sln", settings => {
		settings.SetConfiguration(configuration);
		settings.SetPlatformTarget(PlatformTarget.x86);
	});
});

Task("Run-Tests")
	.IsDependentOn("Build")
	.Does(() => 
{
	NUnit3("./Tests/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
		StopOnError = true,
		X86 = true
    });
});

Task("MoveArtifacts")
	.IsDependentOn("Run-Tests")
    .Does(() =>
{
    CreateDirectory("artifacts");
    MoveFile("SaveClicks/bin/x86/" + configuration + "/SaveClicks.vsix", "artifacts/SaveClicks." + version + ".vsix");
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("MoveArtifacts");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);