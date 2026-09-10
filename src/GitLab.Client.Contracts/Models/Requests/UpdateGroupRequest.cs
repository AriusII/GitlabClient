using GitLab.Client.Domain;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /groups/:id</c>.
///     <para>
///         Every member is optional: nulls are omitted from the payload rather than sent, so an update
///         touches only the settings it names and never clears one by accident. The group is addressed by
///         the route, so there is no id member; the avatar is a binary part and goes through
///         <c>IGroupsClient.SetAvatarAsync</c> instead.
///     </para>
/// </summary>
public sealed record UpdateGroupRequest
{
    /// <summary>The group's display name.</summary>
    public string? Name { get; init; }

    /// <summary>The group's URL slug, unique within its parent.</summary>
    public string? Path { get; init; }

    public string? Description { get; init; }

    public GitLabVisibility? Visibility { get; init; }

    /// <summary>Stops projects in this group from being shared with another group.</summary>
    public bool? ShareWithGroupLock { get; init; }

    public bool? RequireTwoFactorAuthentication { get; init; }

    /// <summary>Hours a member has to set up two-factor authentication before it is enforced.</summary>
    public int? TwoFactorGracePeriod { get; init; }

    public GitLabGroupProjectCreationLevel? ProjectCreationLevel { get; init; }

    public bool? AutoDevopsEnabled { get; init; }

    public GitLabGroupSubgroupCreationLevel? SubgroupCreationLevel { get; init; }

    /// <summary>GitLab also accepts the inverted <c>emails_disabled</c>, deprecated in 16.5; only this one is offered here.</summary>
    public bool? EmailsEnabled { get; init; }

    public bool? ShowDiffPreviewInEmail { get; init; }

    /// <summary>Stops the group itself from being mentioned with an at-sign.</summary>
    public bool? MentionsDisabled { get; init; }

    public bool? LfsEnabled { get; init; }

    public bool? RequestAccessEnabled { get; init; }

    /// <summary>Default branch for projects created in this group.</summary>
    public string? DefaultBranch { get; init; }

    /// <summary>
    ///     GitLab's legacy numeric protection level (0 none, 1 partial, 2 full, 3 push-only, 4 initial-push). Superseded
    ///     by <see cref="DefaultBranchProtectionDefaults" />.
    /// </summary>
    public int? DefaultBranchProtection { get; init; }

    public GitLabDefaultBranchProtectionDefaults? DefaultBranchProtectionDefaults { get; init; }

    public GitLabGroupGitAccessProtocol? EnabledGitAccessProtocol { get; init; }

    /// <summary>Customer relations management.</summary>
    public bool? CrmEnabled { get; init; }

    public bool? ResourceAccessTokenNotifyInherited { get; init; }

    public bool? LockResourceAccessTokenNotifyInherited { get; init; }

    /// <summary>Stops new members being added to projects inside this group.</summary>
    public bool? MembershipLock { get; init; }

    public string? LdapCn { get; init; }

    /// <summary>Access level granted to members of the LDAP group named by <see cref="LdapCn" />.</summary>
    public int? LdapAccess { get; init; }

    /// <summary>Administrators only.</summary>
    public int? SharedRunnersMinutesLimit { get; init; }

    /// <summary>Administrators only.</summary>
    public int? ExtraSharedRunnersMinutesLimit { get; init; }

    public GitLabGroupWikiAccessLevel? WikiAccessLevel { get; init; }

    public GitLabGroupDuoAvailability? DuoAvailability { get; init; }

    public bool? DuoRemoteFlowsAvailability { get; init; }

    public bool? DuoFoundationalFlowsAvailability { get; init; }

    public bool? DuoCustomAgentsAvailability { get; init; }

    public bool? DuoCustomFlowsAvailability { get; init; }

    public bool? DuoExternalAgentsAvailability { get; init; }

    public GitLabGroupToolApprovalAvailability? ToolApprovalForSessionAvailability { get; init; }

    public bool? AmazonQAutoReviewEnabled { get; init; }

    public bool? ExperimentFeaturesEnabled { get; init; }

    public bool? ModelPromptCacheEnabled { get; init; }

    /// <summary>Whether each named foundational agent is enabled for this group.</summary>
    public IReadOnlyList<GitLabFoundationalAgentStatus>? FoundationalAgentsStatuses { get; init; }

    /// <summary>AI-related settings for this group.</summary>
    public GitLabAiSettingsAttributes? AiSettingsAttributes { get; init; }

    public GitLabGroupSharedRunnersSetting? SharedRunnersSetting { get; init; }

    /// <summary>Top-level groups only.</summary>
    public bool? PreventSharingGroupsOutsideHierarchy { get; init; }

    public string? StepUpAuthRequiredOauthProvider { get; init; }

    public bool? MathRenderingLimitsEnabled { get; init; }

    public bool? LockMathRenderingLimitsEnabled { get; init; }

    /// <summary>Maximum job artifact size in megabytes.</summary>
    public int? MaxArtifactsSize { get; init; }

    /// <summary>Project the group's file templates are read from.</summary>
    public long? FileTemplateProjectId { get; init; }

    public bool? PreventForkingOutsideGroup { get; init; }

    /// <summary>Projects a member may download before being banned.</summary>
    public int? UniqueProjectDownloadLimit { get; init; }

    public int? UniqueProjectDownloadLimitIntervalInSeconds { get; init; }

    /// <summary>Usernames exempt from the download limit.</summary>
    public IReadOnlyList<string>? UniqueProjectDownloadLimitAllowlist { get; init; }

    /// <summary>User IDs to alert when the download limit trips.</summary>
    public IReadOnlyList<long>? UniqueProjectDownloadLimitAlertlist { get; init; }

    public bool? AutoBanUserOnExcessiveProjectsDownload { get; init; }

    /// <summary>Comma-separated CIDR ranges access is restricted to.</summary>
    public string? IpRestrictionRanges { get; init; }

    /// <summary>Comma-separated email domains members must belong to.</summary>
    public string? AllowedEmailDomainsList { get; init; }

    public bool? ServiceAccessTokensExpirationEnforced { get; init; }

    public bool? DuoCoreFeaturesEnabled { get; init; }

    public bool? DuoFeaturesEnabled { get; init; }

    public bool? LockDuoFeaturesEnabled { get; init; }

    public bool? AutoDuoCodeReviewEnabled { get; init; }

    public bool? AiAuditEventsStorageEnabled { get; init; }

    public bool? LockAiAuditEventsStorageEnabled { get; init; }

    public bool? WebBasedCommitSigningEnabled { get; init; }

    public bool? OnlyAllowMergeIfPipelineSucceeds { get; init; }

    public bool? AllowMergeOnSkippedPipeline { get; init; }

    public bool? OnlyAllowMergeIfAllDiscussionsAreResolved { get; init; }

    public IReadOnlyList<string>? EnabledFoundationalFlows { get; init; }

    public long? DuoTemplateProjectId { get; init; }

    public bool? AllowPersonalSnippets { get; init; }

    /// <summary>Namespace-based rules that grant access to GitLab Duo features.</summary>
    public IReadOnlyList<GitLabDuoNamespaceAccessRule>? DuoNamespaceAccessRules { get; init; }

    public bool? BuiltInProjectTemplatesEnabled { get; init; }

    public bool? LockBuiltInProjectTemplatesEnabled { get; init; }

    public bool? CreateCodeReviewFlowConsent { get; init; }
}