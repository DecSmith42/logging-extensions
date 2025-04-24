namespace Atom;

[TargetDefinition]
internal partial interface IPackFileLogging : IDotnetPackHelper
{
    const string FileLoggingProjectName = "DecSm.Extensions.Logging.File";

    Target PackFileLogging =>
        d => d
            .WithDescription("Builds the DecSm.Extensions.Logging.File project into a NuGet package")
            .ProducesArtifact(FileLoggingProjectName)
            .Executes(async () => await DotnetPackProject(new(FileLoggingProjectName)));
}
