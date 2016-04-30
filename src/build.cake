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
// .\build.ps1 --revision="1" --productVersion="1.0"
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var revision = Argument("revision", "1");
var version = Argument("productVersion", "1.0");

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
    var fileVersion = version + "." + DateTime.UtcNow.ToString("yy") + DateTime.UtcNow.DayOfYear + "." + revision;
	var infoVersion = version;
	
    Information("Version: " + version);
	Information("FileVersion: " + fileVersion);
    Information("InfoVersion: " + infoVersion);
		
    ReplaceTextInFiles("./**/Properties/AssemblyInfo.cs",
                        "0.0",
                        version);
                        
    ReplaceTextInFiles("./**/Properties/AssemblyInfo.cs",
                        "0.0.0",
                        fileVersion);
						
	ReplaceTextInFiles("./**/Properties/AssemblyInfo.cs",
                        "0.0.0.0",
                        infoVersion);					
});

Task("Build")
    .IsDependentOn("PatchVersion")
    .Does(() =>
{
    MSBuild("./SaveClicks.sln", settings =>
        settings.SetConfiguration(configuration));
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);