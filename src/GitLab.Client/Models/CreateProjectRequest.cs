using GitLab.Client.Domain;

namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects</c> and <c>POST /projects/user/:user_id</c>.
/// </summary>
/// <remarks>
///     Every member is optional so a caller sends only what it means to set - unset members are omitted
///     from the payload rather than written as null. The spec declares the endpoint as
///     <c>multipart/form-data</c> only because of its binary <c>avatar</c> field; the field set is
///     otherwise plain JSON, and the avatar has its own multipart call
///     (<c>IProjectsClient.SetAvatarAsync</c>). Fields the spec marks deprecated in favour of a named
///     replacement (<c>tag_list</c>, <c>public_builds</c>, <c>emails_disabled</c>,
///     <c>packages_enabled</c>, <c>container_registry_enabled</c>) are deliberately absent.
/// </remarks>
public sealed record CreateProjectRequest
{
    /// <summary>The name of the project.</summary>
    public string? Name { get; init; }

    /// <summary>The path of the repository.</summary>
    public string? Path { get; init; }

    /// <summary>The default branch of the project.</summary>
    public string? DefaultBranch { get; init; }

    /// <summary>The description of the project.</summary>
    public string? Description { get; init; }

    /// <summary>The Git strategy. Defaults to `fetch`.</summary>
    public GitLabBuildGitStrategy? BuildGitStrategy { get; init; }

    /// <summary>Build timeout.</summary>
    public int? BuildTimeout { get; init; }

    /// <summary>Auto-cancel pending pipelines.</summary>
    public GitLabAutoCancelPendingPipelines? AutoCancelPendingPipelines { get; init; }

    /// <summary>The path to CI config file. Defaults to `.gitlab-ci.yml`.</summary>
    public string? CiConfigPath { get; init; }

    /// <summary>Disable or enable the service desk.</summary>
    public bool? ServiceDeskEnabled { get; init; }

    /// <summary>Flag indication if the issue tracker is enabled.</summary>
    public bool? IssuesEnabled { get; init; }

    /// <summary>Flag indication if merge requests are enabled.</summary>
    public bool? MergeRequestsEnabled { get; init; }

    /// <summary>Flag indication if the wiki is enabled.</summary>
    public bool? WikiEnabled { get; init; }

    /// <summary>Flag indication if jobs are enabled.</summary>
    public bool? JobsEnabled { get; init; }

    /// <summary>Flag indication if snippets are enabled.</summary>
    public bool? SnippetsEnabled { get; init; }

    /// <summary>Issues access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? IssuesAccessLevel { get; init; }

    /// <summary>Repository access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? RepositoryAccessLevel { get; init; }

    /// <summary>Merge requests access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? MergeRequestsAccessLevel { get; init; }

    /// <summary>Forks access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? ForkingAccessLevel { get; init; }

    /// <summary>Wiki access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? WikiAccessLevel { get; init; }

    /// <summary>Builds access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? BuildsAccessLevel { get; init; }

    /// <summary>Snippets access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? SnippetsAccessLevel { get; init; }

    /// <summary>
    ///     Controls visibility of the package registry. One of `disabled`, `private`, `enabled` or `public`.
    ///     `private` will make the package registry ...
    /// </summary>
    public GitLabProjectPublicFeatureAccessLevel? PackageRegistryAccessLevel { get; init; }

    /// <summary>Pages access level. One of `disabled`, `private`, `enabled` or `public`.</summary>
    public GitLabProjectPublicFeatureAccessLevel? PagesAccessLevel { get; init; }

    /// <summary>Analytics access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? AnalyticsAccessLevel { get; init; }

    /// <summary>
    ///     Controls visibility of the container registry. One of `disabled`, `private` or `enabled`. `private`
    ///     will make the container registry access...
    /// </summary>
    public GitLabProjectFeatureAccessLevel? ContainerRegistryAccessLevel { get; init; }

    /// <summary>Security and compliance access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? SecurityAndComplianceAccessLevel { get; init; }

    /// <summary>Releases access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? ReleasesAccessLevel { get; init; }

    /// <summary>Environments access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? EnvironmentsAccessLevel { get; init; }

    /// <summary>Feature flags access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? FeatureFlagsAccessLevel { get; init; }

    /// <summary>Infrastructure access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? InfrastructureAccessLevel { get; init; }

    /// <summary>Monitor access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? MonitorAccessLevel { get; init; }

    /// <summary>Model experiments access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? ModelExperimentsAccessLevel { get; init; }

    /// <summary>Model registry access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? ModelRegistryAccessLevel { get; init; }

    /// <summary>Enable email notifications.</summary>
    public bool? EmailsEnabled { get; init; }

    /// <summary>Show default award emojis.</summary>
    public bool? ShowDefaultAwardEmojis { get; init; }

    /// <summary>Include the code diff preview in merge request notification emails.</summary>
    public bool? ShowDiffPreviewInEmail { get; init; }

    /// <summary>Warn about potentially unwanted characters.</summary>
    public bool? WarnAboutPotentiallyUnwantedCharacters { get; init; }

    /// <summary>Enforce auth check on uploads.</summary>
    public bool? EnforceAuthChecksOnUploads { get; init; }

    /// <summary>Flag indication if shared runners are enabled for that project.</summary>
    public bool? SharedRunnersEnabled { get; init; }

    /// <summary>Flag indication if group runners are enabled for that project.</summary>
    public bool? GroupRunnersEnabled { get; init; }

    /// <summary>The process mode of the resource group.</summary>
    public GitLabResourceGroupProcessMode? ResourceGroupDefaultProcessMode { get; init; }

    /// <summary>Automatically resolve merge request diff threads on lines changed with a push.</summary>
    public bool? ResolveOutdatedDiffDiscussions { get; init; }

    /// <summary>Remove the source branch by default after merge.</summary>
    public bool? RemoveSourceBranchAfterMerge { get; init; }

    /// <summary>Object that contains information on the container expiration policy.</summary>
    public ContainerExpirationPolicyAttributes? ContainerExpirationPolicyAttributes { get; init; }

    /// <summary>Flag indication if Git LFS is enabled for that project.</summary>
    public bool? LfsEnabled { get; init; }

    /// <summary>The visibility of the project.</summary>
    public GitLabVisibility? Visibility { get; init; }

    /// <summary>Perform public builds.</summary>
    public bool? PublicJobs { get; init; }

    /// <summary>Allow users to request member access.</summary>
    public bool? RequestAccessEnabled { get; init; }

    /// <summary>Only allow to merge if builds succeed.</summary>
    public bool? OnlyAllowMergeIfPipelineSucceeds { get; init; }

    /// <summary>Allow to merge if pipeline is skipped.</summary>
    public bool? AllowMergeOnSkippedPipeline { get; init; }

    /// <summary>Only allow to merge if all threads are resolved.</summary>
    public bool? OnlyAllowMergeIfAllDiscussionsAreResolved { get; init; }

    /// <summary>The list of topics for a project.</summary>
    public IReadOnlyList<string>? Topics { get; init; }

    /// <summary>Show link to create/view merge request when pushing from the command line.</summary>
    public bool? PrintingMergeRequestLinkEnabled { get; init; }

    /// <summary>The merge method used when merging merge requests.</summary>
    public GitLabMergeMethod? MergeMethod { get; init; }

    /// <summary>The commit message used to apply merge request suggestions.</summary>
    public string? SuggestionCommitMessage { get; init; }

    /// <summary>Template used to create merge commit message.</summary>
    public string? MergeCommitTemplate { get; init; }

    /// <summary>Template used to create squash commit message.</summary>
    public string? SquashCommitTemplate { get; init; }

    /// <summary>Template used to create a branch from an issue.</summary>
    public string? IssueBranchTemplate { get; init; }

    /// <summary>Flag indication if Auto DevOps is enabled.</summary>
    public bool? AutoDevopsEnabled { get; init; }

    /// <summary>Auto Deploy strategy.</summary>
    public GitLabAutoDevopsDeployStrategy? AutoDevopsDeployStrategy { get; init; }

    /// <summary>Flag indication if referenced issues auto-closing is enabled.</summary>
    public bool? AutocloseReferencedIssues { get; init; }

    /// <summary>Which storage shard the repository is on. Available only to admins.</summary>
    public string? RepositoryStorage { get; init; }

    /// <summary>Squash default for project. One of `never`, `always`, `default_on`, or `default_off`.</summary>
    public GitLabSquashOption? SquashOption { get; init; }

    /// <summary>Merge requests of this forked project targets itself by default.</summary>
    public bool? MrDefaultTargetSelf { get; init; }

    /// <summary>Template used to generate the default merge request title. Maximum 100 characters.</summary>
    public string? MrDefaultTitleTemplate { get; init; }

    /// <summary>Set whether the project is a CI/CD catalog project.</summary>
    public bool? CicdCatalogEnabled { get; init; }

    /// <summary>Blocks merge requests from merging unless all status checks have passed.</summary>
    public bool? OnlyAllowMergeIfAllStatusChecksPassed { get; init; }

    /// <summary>How many approvers should approve merge request by default.</summary>
    public int? ApprovalsBeforeMerge { get; init; }

    /// <summary>[Deprecated] Enables pull mirroring in a project.</summary>
    public bool? Mirror { get; init; }

    /// <summary>[Deprecated] Pull mirroring triggers builds.</summary>
    public bool? MirrorTriggerBuilds { get; init; }

    /// <summary>The classification label for the project.</summary>
    public string? ExternalAuthorizationClassificationLabel { get; init; }

    /// <summary>Requirements feature access level. One of `disabled`, `private` or `enabled`.</summary>
    public GitLabProjectFeatureAccessLevel? RequirementsAccessLevel { get; init; }

    /// <summary>Require an associated issue from Jira.</summary>
    public bool? PreventMergeWithoutJiraIssue { get; init; }

    /// <summary>Enable automatic reviews by GitLab Duo on merge requests.</summary>
    public bool? AutoDuoCodeReviewEnabled { get; init; }

    /// <summary>Enable GitLab Duo remote flows for this project.</summary>
    public bool? DuoRemoteFlowsEnabled { get; init; }

    /// <summary>Enable GitLab Duo SAST false positive detection for this project.</summary>
    public bool? DuoSastFpDetectionEnabled { get; init; }

    /// <summary>Enable GitLab Duo Secret Detection false positive detection for this project.</summary>
    public bool? DuoSecretDetectionFpEnabled { get; init; }

    /// <summary>Turn on Agentic Breaking Change Resolution for this project.</summary>
    public bool? DuoDependencyBumpBreakingChangesEnabled { get; init; }

    /// <summary>Enable GitLab Duo SAST vulnerability resolution workflow for this project.</summary>
    public bool? DuoSastVrWorkflowEnabled { get; init; }

    /// <summary>Grant read-only access to security policy configurations for enforcement in linked CI/CD projects.</summary>
    public bool? SppRepositoryPipelineAccess { get; init; }

    /// <summary>The regex the Merge Request must adhere to.</summary>
    public string? MergeRequestTitleRegex { get; init; }

    /// <summary>The description for the regex the Merge Request must adhere to.</summary>
    public string? MergeRequestTitleRegexDescription { get; init; }

    /// <summary>The object format of the project repository.</summary>
    public GitLabRepositoryObjectFormat? RepositoryObjectFormat { get; init; }

    /// <summary>Initialize a project with a README.md.</summary>
    public bool? InitializeWithReadme { get; init; }

    /// <summary>Use custom template.</summary>
    public bool? UseCustomTemplate { get; init; }

    /// <summary>Group ID that serves as the template source.</summary>
    public long? GroupWithProjectTemplatesId { get; init; }

    /// <summary>Namespace ID for the new project. Default to the user namespace.</summary>
    public long? NamespaceId { get; init; }

    /// <summary>URL from which the project is imported. Mutually exclusive with `template_name`, `template_project_id`.</summary>
    public Uri? ImportUrl { get; init; }

    /// <summary>
    ///     Name of template from which to create project. Mutually exclusive with `import_url`,
    ///     `template_project_id`.
    /// </summary>
    public string? TemplateName { get; init; }

    /// <summary>
    ///     Project ID of template from which to create project. Mutually exclusive with `import_url`,
    ///     `template_name`.
    /// </summary>
    public long? TemplateProjectId { get; init; }
}