namespace GitLab.Client.Models;

/// <summary>
///     One principal or role permitted to create a protected tag. This is the full GitLab
///     <c>APIEntitiesProtectedRefAccess</c> response shape used by
///     <see cref="GitLabProtectedTag.CreateAccessLevels" />.
/// </summary>
public sealed record GitLabProtectedTagAccessLevel
{
    /// <summary>GitLab's identifier for this protected-ref access entry.</summary>
    public long? Id { get; init; }

    /// <summary>Role access level, when this entry grants access by role rather than by a named principal.</summary>
    public int? AccessLevel { get; init; }

    /// <summary>GitLab's human-readable description of <see cref="AccessLevel" />.</summary>
    public string? AccessLevelDescription { get; init; }

    /// <summary>ID of the deploy key permitted to create the protected tag.</summary>
    public long? DeployKeyId { get; init; }

    /// <summary>ID of the user permitted to create the protected tag.</summary>
    public long? UserId { get; init; }

    /// <summary>ID of the group permitted to create the protected tag.</summary>
    public long? GroupId { get; init; }

    /// <summary>ID of the custom member role associated with this entry, when GitLab supplies one.</summary>
    public long? MemberRoleId { get; init; }

    /// <summary>GitLab's display name for <see cref="MemberRoleId" />.</summary>
    public string? MemberRoleName { get; init; }
}