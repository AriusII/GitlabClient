using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

using GitLab.Client.Domain;

namespace GitLab.Client.Models;

/// <summary>
///     A GitLab project as returned by the Projects API and embedded by resources that reference a project.
/// </summary>
/// <remarks>
///     This is the union of GitLab 19.4's <c>APIEntitiesBasicProjectDetails</c> and
///     <c>APIEntitiesProjectsWithAccessAndCatalogSetting</c> schemas. Neither schema declares required
///     members: GitLab intentionally varies the projection by endpoint and by the caller's permissions.
///     Every property is therefore optional, including the identity fields that happen to be present on a
///     normal <c>GET /projects/:id</c> response.
/// </remarks>
public sealed record GitLabProject
{
    public long? Id { get; init; }

    public string? Description { get; init; }

    public string? Name { get; init; }

    public string? NameWithNamespace { get; init; }

    public string? Path { get; init; }

    public string? PathWithNamespace { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? DefaultBranch { get; init; }

    public IReadOnlyList<string>? TagList { get; init; }

    public IReadOnlyList<string>? Topics { get; init; }

    /// <summary>
    ///     The SSH clone location. GitLab uses the scp-like <c>git@host:namespace/project.git</c> form,
    ///     which is not a portable <see cref="Uri" /> representation.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab's documented SSH clone value is scp-like rather than a URI.")]
    public string? SshUrlToRepo { get; init; }

    public Uri? HttpUrlToRepo { get; init; }

    public Uri? WebUrl { get; init; }

    public Uri? ReadmeUrl { get; init; }

    public int? ForksCount { get; init; }

    public Uri? LicenseUrl { get; init; }

    public GitLabProjectLicense? License { get; init; }

    public Uri? AvatarUrl { get; init; }

    public int? StarCount { get; init; }

    public DateTimeOffset? LastActivityAt { get; init; }

    public GitLabVisibility? Visibility { get; init; }

    public GitLabProjectNamespace? Namespace { get; init; }

    public GitLabProjectCustomAttributeEntry? CustomAttributes { get; init; }

    public string? RepositoryStorage { get; init; }

    public GitLabProject? ForkedFromProject { get; init; }

    public string? ContainerRegistryImagePrefix { get; init; }

    [JsonPropertyName("_links")] public GitLabProjectLinks? Links { get; init; }

    public DateTimeOffset? MarkedForDeletionAt { get; init; }

    public DateTimeOffset? MarkedForDeletionOn { get; init; }

    public bool? PackagesEnabled { get; init; }

    public bool? EmptyRepo { get; init; }

    public bool? Archived { get; init; }

    public GitLabProjectOwner? Owner { get; init; }

    public bool? ResolveOutdatedDiffDiscussions { get; init; }

    public GitLabProjectContainerExpirationPolicy? ContainerExpirationPolicy { get; init; }

    public string? RepositoryObjectFormat { get; init; }

    public bool? IssuesEnabled { get; init; }

    public bool? MergeRequestsEnabled { get; init; }

    public bool? WikiEnabled { get; init; }

    public bool? JobsEnabled { get; init; }

    public bool? SnippetsEnabled { get; init; }

    public bool? ContainerRegistryEnabled { get; init; }

    public bool? ServiceDeskEnabled { get; init; }

    public string? ServiceDeskAddress { get; init; }

    public bool? CanCreateMergeRequestIn { get; init; }

    public string? IssuesAccessLevel { get; init; }

    public string? RepositoryAccessLevel { get; init; }

    public string? MergeRequestsAccessLevel { get; init; }

    public string? ForkingAccessLevel { get; init; }

    public string? WikiAccessLevel { get; init; }

    public string? BuildsAccessLevel { get; init; }

    public string? SnippetsAccessLevel { get; init; }

    public string? PagesAccessLevel { get; init; }

    public string? AnalyticsAccessLevel { get; init; }

    public string? ContainerRegistryAccessLevel { get; init; }

    public string? SecurityAndComplianceAccessLevel { get; init; }

    public string? ReleasesAccessLevel { get; init; }

    public string? EnvironmentsAccessLevel { get; init; }

    public string? FeatureFlagsAccessLevel { get; init; }

    public string? InfrastructureAccessLevel { get; init; }

    public string? MonitorAccessLevel { get; init; }

    public string? ModelExperimentsAccessLevel { get; init; }

    public string? ModelRegistryAccessLevel { get; init; }

    public string? PackageRegistryAccessLevel { get; init; }

    public bool? EmailsDisabled { get; init; }

    public bool? EmailsEnabled { get; init; }

    public bool? ShowDiffPreviewInEmail { get; init; }

    public bool? SharedRunnersEnabled { get; init; }

    public bool? LfsEnabled { get; init; }

    public long? CreatorId { get; init; }

    public bool? MrDefaultTargetSelf { get; init; }

    public Uri? ImportUrl { get; init; }

    public string? ImportType { get; init; }

    public string? ImportStatus { get; init; }

    public string? ImportError { get; init; }

    public int? OpenIssuesCount { get; init; }

    public string? DescriptionHtml { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public int? CiDefaultGitDepth { get; init; }

    public int? CiDeletePipelinesInSeconds { get; init; }

    public bool? CiForwardDeploymentEnabled { get; init; }

    public bool? CiForwardDeploymentRollbackAllowed { get; init; }

    public bool? CiJobTokenScopeEnabled { get; init; }

    public bool? CiSeparatedCaches { get; init; }

    public bool? CiAllowForkPipelinesToRunInParentProject { get; init; }

    public IReadOnlyList<string>? CiIdTokenSubClaimComponents { get; init; }

    public string? BuildGitStrategy { get; init; }

    public bool? KeepLatestArtifact { get; init; }

    public bool? RestrictUserDefinedVariables { get; init; }

    public string? CiPipelineVariablesMinimumOverrideRole { get; init; }

    public int? RunnerTokenExpirationInterval { get; init; }

    public bool? GroupRunnersEnabled { get; init; }

    public string? ResourceGroupDefaultProcessMode { get; init; }

    public string? AutoCancelPendingPipelines { get; init; }

    public int? BuildTimeout { get; init; }

    public bool? AutoDevopsEnabled { get; init; }

    public string? AutoDevopsDeployStrategy { get; init; }

    public bool? CiPushRepositoryForJobTokenAllowed { get; init; }

    public bool? ProtectMergeRequestPipelines { get; init; }

    public bool? CiDisplayPipelineVariables { get; init; }

    public string? RunnersToken { get; init; }

    public string? CiConfigPath { get; init; }

    public bool? PublicJobs { get; init; }

    /// <summary>
    ///     Groups with which the project is shared. GitLab declares the array items as unshaped objects,
    ///     so they remain raw JSON rather than an invented contract.
    /// </summary>
    public IReadOnlyList<JsonElement>? SharedWithGroups { get; init; }

    public bool? OnlyAllowMergeIfPipelineSucceeds { get; init; }

    public bool? AllowMergeOnSkippedPipeline { get; init; }

    public bool? RequestAccessEnabled { get; init; }

    public bool? OnlyAllowMergeIfAllDiscussionsAreResolved { get; init; }

    public bool? RemoveSourceBranchAfterMerge { get; init; }

    public bool? PrintingMergeRequestLinkEnabled { get; init; }

    public string? MergeMethod { get; init; }

    public string? SquashOption { get; init; }

    public bool? AutomaticRebaseEnabled { get; init; }

    public bool? EnforceAuthChecksOnUploads { get; init; }

    public string? SuggestionCommitMessage { get; init; }

    public string? MergeCommitTemplate { get; init; }

    public string? SquashCommitTemplate { get; init; }

    public string? MrDefaultTitleTemplate { get; init; }

    public string? IssueBranchTemplate { get; init; }

    public GitLabProjectStatistics? Statistics { get; init; }

    public bool? WarnAboutPotentiallyUnwantedCharacters { get; init; }

    public bool? AutocloseReferencedIssues { get; init; }

    public int? MaxArtifactsSize { get; init; }

    public string? ApprovalsBeforeMerge { get; init; }

    public string? Mirror { get; init; }

    public string? MirrorUserId { get; init; }

    public string? MirrorTriggerBuilds { get; init; }

    public string? OnlyMirrorProtectedBranches { get; init; }

    public string? MirrorOverwritesDivergedBranches { get; init; }

    public string? ExternalAuthorizationClassificationLabel { get; init; }

    public string? RequirementsEnabled { get; init; }

    public string? RequirementsAccessLevel { get; init; }

    public string? SecurityAndComplianceEnabled { get; init; }

    public bool? SecretPushProtectionEnabled { get; init; }

    public bool? PreReceiveSecretDetectionEnabled { get; init; }

    public string? ComplianceFrameworks { get; init; }

    public string? IssuesTemplate { get; init; }

    public string? MergeRequestsTemplate { get; init; }

    public string? CiRestrictPipelineCancellationRole { get; init; }

    public string? MergePipelinesEnabled { get; init; }

    public string? MergeTrainsEnabled { get; init; }

    public string? MergeTrainsSkipTrainAllowed { get; init; }

    public string? MergeTrainEnforcement { get; init; }

    public string? MaxPipelinesPerMergeTrain { get; init; }

    public string? OnlyAllowMergeIfAllStatusChecksPassed { get; init; }

    public bool? AllowPipelineTriggerApproveDeployment { get; init; }

    public string? PreventMergeWithoutJiraIssue { get; init; }

    public string? AutoDuoCodeReviewEnabled { get; init; }

    public string? ReviewerAssignmentStrategy { get; init; }

    public string? DuoRemoteFlowsEnabled { get; init; }

    public string? DuoFoundationalFlowsEnabled { get; init; }

    public string? DuoSastFpDetectionEnabled { get; init; }

    public string? DuoSecretDetectionFpEnabled { get; init; }

    public string? DuoDependencyBumpBreakingChangesEnabled { get; init; }

    public string? DuoSastVrWorkflowEnabled { get; init; }

    public string? WebBasedCommitSigningEnabled { get; init; }

    public bool? SppRepositoryPipelineAccess { get; init; }

    public bool? SecurityPolicyPipelineMustSucceed { get; init; }

    public string? MergeRequestTitleRegex { get; init; }

    public string? MergeRequestTitleRegexDescription { get; init; }

    public GitLabProjectPermissions? Permissions { get; init; }

    [JsonPropertyName("cicd_catalog_enabled")]
    public bool? CiCdCatalogEnabled { get; init; }
}