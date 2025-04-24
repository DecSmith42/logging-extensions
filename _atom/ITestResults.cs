namespace Atom;

[TargetDefinition]
internal partial interface ITestFileLogging : IDotnetTestHelper
{
    const string FileLoggingTestProjectName = "DecSm.Extensions.Logging.File.UnitTests";

    Target TestFileLogging =>
        d => d
            .WithDescription("Runs the DecSm.Extensions.Logging.File.UnitTests tests")
            .ProducesArtifact(FileLoggingTestProjectName)
            .Executes(async () => await RunDotnetUnitTests(new(FileLoggingTestProjectName)));
}
