namespace GitLab.Client.Models;

/// <summary>
///     A single protected-ref access rule, as returned in the push, merge, unprotect, or protected-tag
///     access collections. A rule selects one principal shape: an access level, user, group, deploy key,
///     or custom member role.
/// </summary>
public sealed record GitLabAccessLevel
{
    /// <summary>The persisted access-rule ID, used when an existing rule is updated or deleted.</summary>
    public long? Id { get; init; }

    /// <summary>
    ///     The role-based access level. It is <see langword="null" /> for principal-specific rules such
    ///     as a deploy key or custom member role.
    /// </summary>
    public int? AccessLevel { get; init; }

    public string? AccessLevelDescription { get; init; }

    /// <summary>The deploy key granted push access. This applies only to push rules.</summary>
    public long? DeployKeyId { get; init; }

    /// <summary>The user granted access by this rule.</summary>
    public long? UserId { get; init; }

    /// <summary>The group granted access by this rule.</summary>
    public long? GroupId { get; init; }

    /// <summary>
    ///     The Ultimate custom member role granted access. GitLab introduced this field in 19.2 behind
    ///     the <c>custom_roles_for_protected_branches</c> feature flag.
    /// </summary>
    public long? MemberRoleId { get; init; }

    /// <summary>The human-readable name of <see cref="MemberRoleId" />, when GitLab includes it.</summary>
    public string? MemberRoleName { get; init; }
}