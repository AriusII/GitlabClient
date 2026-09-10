using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     A group projection returned by GitLab's Groups API. GitLab 19.4 defines both group response
///     schemas without a <c>required</c> list, so every member remains nullable: a group can be embedded
///     at different widths by the routes that return it.
/// </summary>
public sealed record GitLabGroup
{
    public long? Id { get; init; }

    public string? Name { get; init; }

    public string? Path { get; init; }

    public string? Description { get; init; }

    public string? Visibility { get; init; }

    public Uri? WebUrl { get; init; }

    public string? FullName { get; init; }

    public string? FullPath { get; init; }

    public Uri? AvatarUrl { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? ParentId { get; init; }

    public long? OrganizationId { get; init; }

    public bool? Archived { get; init; }

    public DateOnly? MarkedForDeletionOn { get; init; }

    public string? DefaultBranch { get; init; }

    public int? DefaultBranchProtection { get; init; }

    public GitLabDefaultBranchProtectionDefaults? DefaultBranchProtectionDefaults { get; init; }

    public string? ProjectCreationLevel { get; init; }

    public string? SubgroupCreationLevel { get; init; }

    public string? SharedRunnersSetting { get; init; }

    public string? WikiAccessLevel { get; init; }

    public string? EnabledGitAccessProtocol { get; init; }

    public string? RepositoryStorage { get; init; }

    public bool? ShareWithGroupLock { get; init; }

    public bool? RequireTwoFactorAuthentication { get; init; }

    public int? TwoFactorGracePeriod { get; init; }

    public bool? EmailsDisabled { get; init; }

    public bool? EmailsEnabled { get; init; }

    public bool? ShowDiffPreviewInEmail { get; init; }

    public string? MentionsDisabled { get; init; }

    public bool? LfsEnabled { get; init; }

    public bool? MathRenderingLimitsEnabled { get; init; }

    public bool? LockMathRenderingLimitsEnabled { get; init; }

    public bool? CrmEnabled { get; init; }

    public bool? ResourceAccessTokenNotifyInherited { get; init; }

    public bool? LockResourceAccessTokenNotifyInherited { get; init; }

    public string? AutoDevopsEnabled { get; init; }

    public bool? RequestAccessEnabled { get; init; }

    public string? MembershipLock { get; init; }

    public bool? PreventSharingGroupsOutsideHierarchy { get; init; }

    public int? MaxArtifactsSize { get; init; }

    public string? FileTemplateProjectId { get; init; }

    public string? RunnersToken { get; init; }

    public string? LdapCn { get; init; }

    public string? LdapAccess { get; init; }

    public GitLabGroupStatistics? Statistics { get; init; }

    public GitLabGroupRootStorageStatistics? RootStorageStatistics { get; init; }

    public GitLabGroupCustomAttributeEntry? CustomAttributes { get; init; }

    public GitLabGroupLdapLink? LdapGroupLinks { get; init; }

    public GitLabGroupSamlLink? SamlGroupLinks { get; init; }

    public bool? DuoCoreFeaturesEnabled { get; init; }

    public string? DuoFeaturesEnabled { get; init; }

    public string? LockDuoFeaturesEnabled { get; init; }

    public string? AutoDuoCodeReviewEnabled { get; init; }

    public string? WebBasedCommitSigningEnabled { get; init; }

    public string? AllowPersonalSnippets { get; init; }

    public string? DuoNamespaceAccessRules { get; init; }

    public bool? BuiltInProjectTemplatesEnabled { get; init; }

    public bool? LockBuiltInProjectTemplatesEnabled { get; init; }

    /// <summary>
    ///     Share entries are objects with no declared properties in the GitLab 19.4 schema. They are kept
    ///     as raw JSON rather than projected to an invented stable shape.
    /// </summary>
    public IReadOnlyList<JsonElement>? SharedWithGroups { get; init; }

    public IReadOnlyList<GitLabProject>? Projects { get; init; }

    public IReadOnlyList<GitLabProject>? SharedProjects { get; init; }

    public string? SharedRunnersMinutesLimit { get; init; }

    public string? ExtraSharedRunnersMinutesLimit { get; init; }

    public string? PreventForkingOutsideGroup { get; init; }

    public string? ServiceAccessTokensExpirationEnforced { get; init; }

    public string? ExperimentFeaturesEnabled { get; init; }

    public GitLabGroupAiSettings? AiSettings { get; init; }

    public string? IpRestrictionRanges { get; init; }

    public string? AllowedEmailDomainsList { get; init; }

    public string? OnlyAllowMergeIfPipelineSucceeds { get; init; }

    public string? AllowMergeOnSkippedPipeline { get; init; }

    public string? OnlyAllowMergeIfAllDiscussionsAreResolved { get; init; }

    public string? UniqueProjectDownloadLimit { get; init; }

    public string? UniqueProjectDownloadLimitIntervalInSeconds { get; init; }

    public string? UniqueProjectDownloadLimitAllowlist { get; init; }

    public string? UniqueProjectDownloadLimitAlertlist { get; init; }

    public string? AutoBanUserOnExcessiveProjectsDownload { get; init; }

    public string? StepUpAuthRequiredOauthProvider { get; init; }
}