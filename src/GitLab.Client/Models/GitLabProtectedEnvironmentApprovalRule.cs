namespace GitLab.Client.Models;

/// <summary>
///     One approval rule on a <see cref="GitLabProtectedEnvironment" />: a user, group or access level
///     that must sign off on a deployment, and how many of its approvals are required.
/// </summary>
public sealed record GitLabProtectedEnvironmentApprovalRule
{
    public long? Id { get; init; }

    public long? UserId { get; init; }

    public long? GroupId { get; init; }

    /// <summary>The role that may approve - 20 Reporter, 30 Developer, 40 Maintainer, 60 Admin.</summary>
    public int? AccessLevel { get; init; }

    public string? AccessLevelDescription { get; init; }

    /// <summary>How many approvals this rule contributes. The modern replacement for the environment-wide count.</summary>
    public int? RequiredApprovals { get; init; }

    /// <summary>0 for direct group membership, 1 for all inherited groups. Null reads as 0.</summary>
    public int? GroupInheritanceType { get; init; }
}