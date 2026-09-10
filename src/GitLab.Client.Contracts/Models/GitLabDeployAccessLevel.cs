namespace GitLab.Client.Models;

/// <summary>
///     One entry in a <see cref="GitLabProtectedEnvironment" />'s deploy access list: a user, a group, a
///     deploy key, a member role, or a bare access level allowed to deploy.
///     <para>
///         Deliberately not <see cref="GitLabAccessLevel" />, which models the much smaller protected-branch
///         shape (an access level and its description, nothing else).
///     </para>
/// </summary>
public sealed record GitLabDeployAccessLevel
{
    public long? Id { get; init; }

    /// <summary>The role allowed to deploy - 20 Reporter, 30 Developer, 40 Maintainer, 60 Admin.</summary>
    public int? AccessLevel { get; init; }

    public string? AccessLevelDescription { get; init; }

    public long? DeployKeyId { get; init; }

    public long? UserId { get; init; }

    public long? GroupId { get; init; }

    public long? MemberRoleId { get; init; }

    public string? MemberRoleName { get; init; }

    /// <summary>0 for direct group membership, 1 for all inherited groups. Null reads as 0.</summary>
    public int? GroupInheritanceType { get; init; }
}