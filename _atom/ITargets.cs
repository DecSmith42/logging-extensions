namespace Atom;

[PublicAPI]
internal interface ITargets : IDotnetPackHelper, IDotnetTestHelper, INugetHelper, IGithubReleaseHelper, ISetupBuildInfo
{
    const string FileLoggingProjectName = "DecSm.Extensions.Logging.File";
    const string FileLoggingTestProjectName = "DecSm.Extensions.Logging.File.UnitTests";

    [ParamDefinition("nuget-push-feed", "The Nuget feed to push to.", "https://api.nuget.org/v3/index.json")]
    string NugetFeed => GetParam(() => NugetFeed, "https://api.nuget.org/v3/index.json");

    [SecretDefinition("nuget-push-api-key", "The API key to use to push to Nuget.")]
    string? NugetApiKey => GetParam(() => NugetApiKey);

    Target PackFileLogging =>
        d => d
            .DescribedAs("Builds the DecSm.Extensions.Logging.File project into a NuGet package")
            .ProducesArtifact(FileLoggingProjectName)
            .Executes(async cancellationToken => await DotnetPackProject(new(FileLoggingProjectName), cancellationToken));

    Target TestFileLogging =>
        d => d
            .DescribedAs("Runs the DecSm.Extensions.Logging.File.UnitTests tests")
            .ProducesArtifact(FileLoggingTestProjectName)
            .Executes(async cancellationToken =>
            {
                var exitCode = 0;

                exitCode += await RunDotnetUnitTests(new(FileLoggingTestProjectName), cancellationToken);

                if (exitCode != 0)
                    throw new StepFailedException("One or more unit tests failed");
            });

    Target PushToNuget =>
        d => d
            .DescribedAs("Pushes the Atom projects to Nuget")
            .ConsumesArtifact(nameof(PackFileLogging), FileLoggingProjectName)
            .RequiresParam(nameof(NugetFeed))
            .RequiresParam(nameof(NugetApiKey))
            .Executes(async cancellationToken => await PushProject(FileLoggingProjectName,
                NugetFeed,
                NugetApiKey!,
                cancellationToken: cancellationToken));

    Target PushToRelease =>
        d => d
            .DescribedAs("Pushes the package to the release feed.")
            .RequiresParam(nameof(GithubToken))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildVersion))
            .ConsumesArtifact(nameof(PackFileLogging), FileLoggingProjectName)
            .Executes(async () =>
            {
                if (BuildVersion.IsPreRelease)
                {
                    Logger.LogInformation("Skipping release push for pre-release version");

                    return;
                }

                await UploadArtifactToRelease(FileLoggingProjectName, $"v{BuildVersion}");
            });
}
