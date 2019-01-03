var target = Argument("Target", "Test");

Task("Restore")
    .Does(() =>
{
    DotNetCoreRestore();
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(()=> 
{
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = "Debug",
        NoRestore = true
    };

    DotNetCoreBuild("./Nybus.sln", settings);
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCoreTestSettings
    {
        NoRestore = true,
        NoBuild = true
    };

    DotNetCoreTest("./Nybus.sln", settings);
});

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
{
    var settings = new DotNetCorePackSettings
    {
        Configuration = "Release",
        NoRestore = true
    };

    DotNetCorePack("./Nybus.sln", settings);
});

RunTarget(target);