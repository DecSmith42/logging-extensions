namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : DefaultBuildDefinition, IGithubWorkflows, IGitVersion, ITargets
{
    public override IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions =>
    [
        UseGitVersionForBuildId.Enabled, new SetupDotnetStep("9.0.x"),
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("Validate")
        {
            Triggers = [GitPullRequestTrigger.IntoMain, ManualTrigger.Empty],
            Targets =
            [
                Targets.SetupBuildInfo,
                Targets.PackFileLogging.WithSuppressedArtifactPublishing,
                Targets
                    .TestFileLogging
                    .WithMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
                    {
                        Values = [IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag],
                    })
                    .WithOptions(GithubRunsOn.SetByMatrix),
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
        new("Build")
        {
            Triggers = [GitPushTrigger.ToMain, GithubReleaseTrigger.OnReleased, ManualTrigger.Empty],
            Targets =
            [
                Targets.SetupBuildInfo,
                Targets.PackFileLogging,
                Targets
                    .TestFileLogging
                    .WithMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
                    {
                        Values = [IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag],
                    })
                    .WithOptions(GithubRunsOn.SetByMatrix),
                Targets.PushToNuget.WithOptions(WorkflowSecretInjection.Create(Params.NugetApiKey)),
                Targets
                    .PushToRelease
                    .WithGithubTokenInjection()
                    .WithOptions(GithubIf.Create(new ConsumedVariableExpression(nameof(Targets.SetupBuildInfo),
                            ParamDefinitions[nameof(ISetupBuildInfo.BuildVersion)].ArgName)
                        .Contains(new StringExpression("-"))
                        .EqualTo("false"))),
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
        Github.DependabotDefaultWorkflow(),
    ];
}
