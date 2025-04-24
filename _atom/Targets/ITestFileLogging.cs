namespace Atom.Targets;

[TargetDefinition]
internal partial interface ITestFileLogging : IDotnetTestHelper
{
    const string FileLoggingTestProjectName = "DecSm.Extensions.Logging.File.UnitTests";

    Target TestFileLogging =>
        d => d
            .WithDescription("Runs the DecSm.Extensions.Logging.File.UnitTests tests")
            .ProducesArtifact(FileLoggingTestProjectName)
            .Executes(async () =>
            {
                var exitCode = 0;

                exitCode += await RunDotnetUnitTests(new(FileLoggingTestProjectName));

                if (exitCode != 0)
                    throw new StepFailedException("One or more unit tests failed");
            });
}
