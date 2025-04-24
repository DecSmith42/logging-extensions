namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : DefaultBuildDefinition,
    IGithubWorkflows,
    IGitVersion,
    IPackFileLogging,
    ITestFileLogging,
    IPushToNuget,
    IPushToRelease
{
    public override IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions =>
    [
        UseGitVersionForBuildId.Enabled, new SetupDotnetStep("9.0.x"),
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("Validate")
        {
            Triggers = [GitPullRequestTrigger.IntoMain, ManualTrigger.Empty],
            StepDefinitions =
            [
                Commands.SetupBuildInfo, Commands.PackFileLogging.WithSuppressedArtifactPublishing, Commands.TestFileLogging,
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
        new("Build")
        {
            Triggers = [GitPushTrigger.ToMain, GithubReleaseTrigger.OnReleased, ManualTrigger.Empty],
            StepDefinitions =
            [
                Commands.SetupBuildInfo,
                Commands.PackFileLogging,
                Commands.TestFileLogging,
                Commands.PushToNuget.WithAddedOptions(WorkflowSecretInjection.Create(Params.NugetApiKey)),
                Commands.PushToRelease.WithGithubTokenInjection(),
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
        Github.DependabotDefaultWorkflow(),
    ];
}
