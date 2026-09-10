using GitLab.Client.Domain;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /groups</c>.
///     <para>
///         Every member but <see cref="Name" /> and <see cref="Path" /> is optional and omitted from the
///         payload when null, so a group is created with GitLab's own defaults for anything left unset.
///         The avatar is not here: it is a binary part, uploaded separately through
///         <c>IGroupsClient.SetAvatarAsync</c>.
///     </para>
/// </summary>
public sealed record CreateGroupRequest
{
    /// <summary>The group's display name.</summary>
    public required string Name { get; init; }

    /// <summary>The group's URL slug, unique within its parent.</summary>
    public required string Path { get; init; }

    /// <summary>Parent group. Omit to create a top-level group.</summary>
    public long? ParentId { get; init; }

    /// <summary>Organization the group belongs to. Defaults to the instance's default organization.</summary>
    public long? OrganizationId { get; init; }

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

    public IReadOnlyList<GitLabFoundationalAgentStatus>? FoundationalAgentsStatuses { get; init; }

    public GitLabAiSettingsAttributes? AiSettingsAttributes { get; init; }
}