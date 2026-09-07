using GitLab.Client.Domain;

namespace GitLab.Client.Models;

/// <summary>
///     A GitLab group, as returned by the "groups" API area.
///     <para>
///         Everything past the first handful of members is nullable on purpose. This type is embedded by
///         other resources and returned by a dozen different group routes, each of which exposes a
///         different slice of the entity - <see cref="Statistics" /> only when the request asked for it,
///         <see cref="RunnersToken" /> only to owners, <see cref="Projects" /> only from the single-group
///         route. Add members here additively and nullably; a new <c>required</c> member would break
///         every embedding at once.
///     </para>
///     <para>
///         The settings GitLab enumerates in its request schemas (<c>project_creation_level</c>,
///         <c>wiki_access_level</c>, <c>shared_runners_setting</c>, ...) are typed as plain strings here
///         even though the matching request members are real enums. The response schema declares them as
///         bare strings, and a value GitLab adds later must not turn a healthy response into a
///         <c>JsonException</c>.
///     </para>
/// </summary>
public sealed record GitLabGroup
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required string Path { get; init; }

    public string? Description { get; init; }

    public required GitLabVisibility Visibility { get; init; }

    public required Uri WebUrl { get; init; }

    public string? FullName { get; init; }

    public string? FullPath { get; init; }

    public Uri? AvatarUrl { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public long? ParentId { get; init; }

    /// <summary>The organization the group belongs to.</summary>
    public long? OrganizationId { get; init; }

    /// <summary>Whether the group is archived. Archived groups are read-only.</summary>
    public bool? Archived { get; init; }

    /// <summary>
    ///     The day the group's deferred deletion runs, set by <c>DELETE /groups/:id</c>. Null for a group
    ///     that is not scheduled for deletion. A plain date, not a timestamp.
    /// </summary>
    public DateOnly? MarkedForDeletionOn { get; init; }

    /// <summary>Default branch for projects created inside this group.</summary>
    public string? DefaultBranch { get; init; }

    /// <summary>
    ///     GitLab's legacy numeric default-branch protection level. Superseded by
    ///     <see cref="DefaultBranchProtectionDefaults" />.
    /// </summary>
    public int? DefaultBranchProtection { get; init; }

    public GitLabDefaultBranchProtectionDefaults? DefaultBranchProtectionDefaults { get; init; }

    /// <summary>
    ///     Who may create projects in the group: <c>noone</c>, <c>owner</c>, <c>maintainer</c>,
    ///     <c>developer</c> or <c>administrator</c>.
    /// </summary>
    public string? ProjectCreationLevel { get; init; }

    /// <summary>Who may create subgroups: <c>owner</c> or <c>maintainer</c>.</summary>
    public string? SubgroupCreationLevel { get; init; }

    /// <summary>One of <c>enabled</c>, <c>disabled_and_overridable</c> or <c>disabled_and_unoverridable</c>.</summary>
    public string? SharedRunnersSetting { get; init; }

    /// <summary>One of <c>disabled</c>, <c>private</c> or <c>enabled</c>.</summary>
    public string? WikiAccessLevel { get; init; }

    /// <summary>One of <c>ssh</c>, <c>http</c> or <c>all</c>.</summary>
    public string? EnabledGitAccessProtocol { get; init; }

    /// <summary>The storage shard the group's repositories live on. Administrators only.</summary>
    public string? RepositoryStorage { get; init; }

    public bool? ShareWithGroupLock { get; init; }

    public bool? RequireTwoFactorAuthentication { get; init; }

    public int? TwoFactorGracePeriod { get; init; }

    public bool? EmailsEnabled { get; init; }

    public bool? ShowDiffPreviewInEmail { get; init; }

    public bool? MentionsDisabled { get; init; }

    public bool? LfsEnabled { get; init; }

    public bool? RequestAccessEnabled { get; init; }

    public bool? CrmEnabled { get; init; }

    public bool? AutoDevopsEnabled { get; init; }

    public bool? MembershipLock { get; init; }

    public bool? PreventSharingGroupsOutsideHierarchy { get; init; }

    /// <summary>Maximum job artifact size in megabytes.</summary>
    public int? MaxArtifactsSize { get; init; }

    /// <summary>Project the group's file templates are read from. Premium and above.</summary>
    public long? FileTemplateProjectId { get; init; }

    /// <summary>The group's runner registration token. Returned to owners only.</summary>
    public string? RunnersToken { get; init; }

    /// <summary>Common Name of the LDAP group this one is synchronised with.</summary>
    public string? LdapCn { get; init; }

    /// <summary>Storage usage. Returned only when the request asked for statistics.</summary>
    public GitLabGroupStatistics? Statistics { get; init; }

    /// <summary>Groups this one has been shared with, through <c>POST /groups/:id/share</c>.</summary>
    public IReadOnlyList<GitLabSharedGroupLink>? SharedWithGroups { get; init; }

    /// <summary>Projects the group owns. Returned by the single-group route only.</summary>
    public IReadOnlyList<GitLabProject>? Projects { get; init; }

    /// <summary>Projects shared into the group. Returned by the single-group route only.</summary>
    public IReadOnlyList<GitLabProject>? SharedProjects { get; init; }
}