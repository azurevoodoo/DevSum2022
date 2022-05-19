public record BuildData(
    DirectoryPath SourcePath,
    DirectoryPath ArtifactsPath,
    string Configuration
)
{
    public DotNetMSBuildSettings MSBuildSettings { get; } = 
        new DotNetMSBuildSettings {
            Version = "2022.5.19.1"
        }
            .SetConfiguration(Configuration);
}

Setup(
    ctx => new BuildData(
        "./src",
        "./artifacts",
        "Release"
    )
);

Task("Clean")
    .Does<BuildData>((context,data)=>{
        CleanDirectory(data.ArtifactsPath);
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does<BuildData>((context,data)=>{
        DotNetRestore(
            data.SourcePath.FullPath,
            new DotNetRestoreSettings {
                MSBuildSettings = data.MSBuildSettings
            }
            );
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does<BuildData>((context,data)=>{
        DotNetBuild(
            data.SourcePath.FullPath,
            new DotNetBuildSettings {
                NoRestore = true,
                MSBuildSettings = data.MSBuildSettings
            }
            );
    });

Task("Test")
    .IsDependentOn("Build")
    .Does<BuildData>((context,data)=>{
        DotNetTest(
            data.SourcePath.FullPath,
            new DotNetTestSettings {
                NoRestore = true,
                NoBuild = true,
                Configuration = data.Configuration
            }
            );
    });

Task("Package")
    .IsDependentOn("Test")
    .Does<BuildData>((context,data)=>{
        DotNetPack(
            data.SourcePath.FullPath,
             new DotNetPackSettings {
                NoRestore = true,
                NoBuild = true,
                OutputDirectory = data.ArtifactsPath,
                MSBuildSettings = data.MSBuildSettings
            }
            );
    });

Task("Publish")
    .IsDependentOn("Package")
    .Does<BuildData>(async (context,data)=>{
        await GitHubActions.Commands.UploadArtifact(
            data.ArtifactsPath,
            $"NuGet{Context.Environment.Platform.Family}"
            );
    });


Task("GitHubActions")
      .IsDependentOn("Publish");

RunTarget(Argument("target", "Package"));