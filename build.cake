//#tool "nuget:https://www.myget.org/F/opencover/api/v3/index.json?package=OpenCover&version=4.6.831-rc"
#tool "nuget:?package=ReportGenerator&version=4.0.5"
#tool "nuget:?package=JetBrains.dotCover.CommandLineTools&version=2018.3.1"

#load "./build/types.cake"

var target = Argument("Target", "Test");

Setup<BuildState>(context => 
{
    var state = new BuildState
    {
        Parameters = new BuildParameters
        {
            CoverageTools = CoverageTool.OpenCover | CoverageTool.DotCover
        },
        Paths = new BuildPaths
        {
            SolutionFile = MakeAbsolute(File("./Nybus.sln"))
        }
    };

    CleanDirectory(state.Paths.OutputFolder);

    return state;
});
Task("Restore")
    .Does<BuildState>(state =>
{
    var settings = new DotNetCoreRestoreSettings
    {

    };

    DotNetCoreRestore(state.Paths.SolutionFile.ToString(), settings);
});

Task("Build")
    .IsDependentOn("Restore")
    .Does<BuildState>(state => 
{
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = "Debug",
        NoRestore = true
    };

    DotNetCoreBuild(state.Paths.SolutionFile.ToString(), settings);
});

Task("Test")
    .IsDependentOn("Build")
    .Does<BuildState>(state =>
{
    var projectFiles = GetFiles($"{state.Paths.TestFolder}/**/*.csproj");

    bool success = true;

    foreach (var file in projectFiles)
    {
        try
        {
            Information($"Testing {file.GetFilenameWithoutExtension()}");

            var testResultFile = state.Paths.TestOutputFolder.CombineWithFilePath(file.GetFilenameWithoutExtension() + ".trx");
            var coverageResultFile = state.Paths.TestOutputFolder.CombineWithFilePath(file.GetFilenameWithoutExtension() + ".dvcr");

            var projectFile = MakeAbsolute(file).ToString();

            var dotCoverSettings = new DotCoverCoverSettings()
                                    .WithFilter("+:Nybus*")
                                    .WithFilter("-:Tests.*");

            var settings = new DotNetCoreTestSettings
            {
                NoBuild = true,
                NoRestore = true,
                Logger = $"trx;LogFileName={testResultFile.FullPath}"
            };

            DotCoverCover(context => 
            {
                context.DotNetCoreTest(projectFile, settings);
            }, coverageResultFile, dotCoverSettings);
        }
        catch (Exception ex)
        {
            Error($"There was an error while executing the tests: {file.GetFilenameWithoutExtension()}", ex);
            success = false;
        }

        Information("");
    }
    
    if (!success)
    {
        throw new CakeException("There was an error while executing the tests");
    }

    Information("Merging coverage files");
    var coverageFiles = GetFiles($"{state.Paths.TestOutputFolder}/*.dvcr");
    DotCoverMerge(coverageFiles, state.Paths.DotCoverOutputFile);
    DeleteFiles(coverageFiles);

    Information("Generating dotCover XML report");
    var xmlFilePath = state.Paths.TestOutputFolder.CombineWithFilePath("coverage.xml");
    DotCoverReport(state.Paths.DotCoverOutputFile, xmlFilePath, new DotCoverReportSettings 
    {
        ReportType = DotCoverReportType.DetailedXML
    });

    Information("Executing ReportGenerator to generate HTML report");
    ReportGenerator(xmlFilePath, state.Paths.ReportFolder, new ReportGeneratorSettings {
            ReportTypes = new[]{ReportGeneratorReportType.Html, ReportGeneratorReportType.Xml}
    });
});

Task("Pack")
    .IsDependentOn("Test")
    .Does<BuildState>(state =>
{
    var settings = new DotNetCorePackSettings
    {
        Configuration = "Release",
        NoRestore = true,
        OutputDirectory = state.Paths.OutputFolder
    };

    DotNetCorePack(state.Paths.SolutionFile.ToString(), settings);
});

Task("Full")
    .IsDependentOn("Pack")
    .IsDependentOn("Test")
    .IsDependentOn("Build")
    .IsDependentOn("Restore");

RunTarget(target);

