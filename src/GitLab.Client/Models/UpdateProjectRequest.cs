using GitLab.Client.Domain;

namespace GitLab.Client.Models;

/// <summary>Request body for <c>PUT /projects/:id</c>.</summary>
/// <remarks>
///     A partial update: every member is optional and unset members are omitted from the payload, so
///     nothing is cleared server-side by accident. It is deliberately a separate type from
///     <see cref="CreateProjectRequest" /> - the update body carries some 30 settings that cannot be
///     supplied at creation time (the CI, mirroring and merge-train families), and cannot carry the
///     creation-only ones (<c>namespace_id</c>, <c>initialize_with_readme</c>, the template fields).
///     Fields the spec marks deprecated in favour of a named replacement are absent; see
///     <see cref="CreateProjectRequest" />.
/// </remarks>
public sealed record UpdateProjectRequest
{
    /// <summary>The name of the project.</summary>
    public string? Name { get; init; }

    /// <summary>The default branch of the project.</summary>
    public string? DefaultBranch { get; init; }

    /// <summary>The path of the repository.</summary>
    public string? Path { get; init; }

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

    /// <summary>Enable Agentic Breaking Change Resolution for this project.</summary>
    public bool? DuoDependencyBumpBreakingChangesEnabled { get; init; }

    /// <summary>Enable GitLab Duo SAST vulnerability resolution workflow for this project.</summary>
    public bool? DuoSastVrWorkflowEnabled { get; init; }

    /// <summary>Grant read-only access to security policy configurations for enforcement in linked CI/CD projects.</summary>
    public bool? SppRepositoryPipelineAccess { get; init; }

    /// <summary>The regex the Merge Request must adhere to.</summary>
    public string? MergeRequestTitleRegex { get; init; }

    /// <summary>The description for the regex the Merge Request must adhere to.</summary>
    public string? MergeRequestTitleRegexDescription { get; init; }

    /// <summary>Default number of revisions for shallow cloning.</summary>
    public int? CiDefaultGitDepth { get; init; }

    /// <summary>Indicates if the latest artifact should be kept for this project.</summary>
    public bool? KeepLatestArtifact { get; init; }

    /// <summary>Prevent older deployment jobs that are still pending.</summary>
    public bool? CiForwardDeploymentEnabled { get; init; }

    /// <summary>Allow job retries for rollback deployments.</summary>
    public bool? CiForwardDeploymentRollbackAllowed { get; init; }

    /// <summary>Allow fork merge request pipelines to run in parent project.</summary>
    public bool? CiAllowForkPipelinesToRunInParentProject { get; init; }

    /// <summary>Enable or disable separated caches based on branch protection.</summary>
    public bool? CiSeparatedCaches { get; init; }

    /// <summary>Restrict use of user-defined variables when triggering a pipeline.</summary>
    public bool? RestrictUserDefinedVariables { get; init; }

    /// <summary>
    ///     Limit ability to override CI/CD variables when triggering a pipeline to only users with at least the
    ///     set minimum role.
    /// </summary>
    public GitLabMinimumRole? CiPipelineVariablesMinimumOverrideRole { get; init; }

    /// <summary>
    ///     Allow pushing to this project's repository by authenticating with a CI/CD job token generated in this
    ///     project.
    /// </summary>
    public bool? CiPushRepositoryForJobTokenAllowed { get; init; }

    /// <summary>Claims that will be used to build the sub claim in id tokens.</summary>
    public IReadOnlyList<string>? CiIdTokenSubClaimComponents { get; init; }

    /// <summary>Pipelines older than the configured time are deleted.</summary>
    public int? CiDeletePipelinesInSeconds { get; init; }

    /// <summary>Set the maximum file size for each job's artifacts.</summary>
    public int? MaxArtifactsSize { get; init; }

    /// <summary>Make protected CI/CD variables and runners available in merge request pipelines.</summary>
    public bool? ProtectMergeRequestPipelines { get; init; }

    /// <summary>Display all manually-defined variables in the pipeline details page after running a pipeline manually.</summary>
    public bool? CiDisplayPipelineVariables { get; init; }

    /// <summary>Enable automatic rebase of the source branch before merge.</summary>
    public bool? AutomaticRebaseEnabled { get; init; }

    /// <summary>
    ///     Limit the ability to create, update, toggle, and delete feature flags to only users with at least the
    ///     set minimum role.
    /// </summary>
    public GitLabMinimumRole? FeatureFlagsMinimumRole { get; init; }

    /// <summary>Allow pipeline triggerer to approve deployments.</summary>
    public bool? AllowPipelineTriggerApproveDeployment { get; init; }

    /// <summary>
    ///     [Deprecated] User responsible for all the activity surrounding a pull mirror event. Can only be set
    ///     by admins.
    /// </summary>
    public long? MirrorUserId { get; init; }

    /// <summary>[Deprecated] Only mirror protected branches. Mutually exclusive with `mirror_branch_regex`.</summary>
    public bool? OnlyMirrorProtectedBranches { get; init; }

    /// <summary>[Deprecated] Only mirror branches match regex. Mutually exclusive with `only_mirror_protected_branches`.</summary>
    public string? MirrorBranchRegex { get; init; }

    /// <summary>[Deprecated] Pull mirror overwrites diverged branches.</summary>
    public bool? MirrorOverwritesDivergedBranches { get; init; }

    /// <summary>URL from which the project is imported.</summary>
    public Uri? ImportUrl { get; init; }

    /// <summary>Overall approvals required when no rule is present.</summary>
    public int? FallbackApprovalsRequired { get; init; }

    /// <summary>Default description for Issues. Description is parsed with GitLab Flavored Markdown.</summary>
    public string? IssuesTemplate { get; init; }

    /// <summary>Default description for merge requests. Description is parsed with GitLab Flavored Markdown.</summary>
    public string? MergeRequestsTemplate { get; init; }

    /// <summary>Enable merged results pipelines.</summary>
    public bool? MergePipelinesEnabled { get; init; }

    /// <summary>Enable merge trains.</summary>
    public bool? MergeTrainsEnabled { get; init; }

    /// <summary>Allow merge train merge requests to be merged without waiting for pipelines to finish.</summary>
    public bool? MergeTrainsSkipTrainAllowed { get; init; }

    /// <summary>
    ///     Merge train enforcement level. One of `allow_bypass`, `enforce_for_all_users`, or
    ///     `enforce_with_owner_override`.
    /// </summary>
    public GitLabMergeTrainEnforcement? MergeTrainEnforcement { get; init; }

    /// <summary>Maximum number of parallel pipelines per merge train for this project.</summary>
    public int? MaxPipelinesPerMergeTrain { get; init; }

    /// <summary>Roles allowed to cancel pipelines and jobs.</summary>
    public string? CiRestrictPipelineCancellationRole { get; init; }

    /// <summary>Enable web based commit signing for this project.</summary>
    public bool? WebBasedCommitSigningEnabled { get; init; }

    /// <summary>Require all security policy pipelines to succeed before merge requests can be merged.</summary>
    public bool? SecurityPolicyPipelineMustSucceed { get; init; }

    /// <summary>Strategy used to automatically assign reviewers to merge requests. One of `disabled` or `code_owners`.</summary>
    public GitLabReviewerAssignmentStrategy? ReviewerAssignmentStrategy { get; init; }
}