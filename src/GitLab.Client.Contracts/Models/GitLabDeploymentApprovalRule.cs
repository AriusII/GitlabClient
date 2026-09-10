namespace GitLab.Client.Models;

/// <summary>
///     One protected-environment approval rule reported as part of a deployment approval summary.
/// </summary>
public sealed record GitLabDeploymentApprovalRule
{
    public long? Id { get; init; }

    public long? UserId { get; init; }

    public long? GroupId { get; init; }

    public int? AccessLevel { get; init; }

    public string? AccessLevelDescription { get; init; }

    public int? RequiredApprovals { get; init; }

    public int? GroupInheritanceType { get; init; }

    /// <summary>The approvals or rejections recorded for this rule, if any.</summary>
    public IReadOnlyList<GitLabDeploymentApproval>? DeploymentApprovals { get; init; }
}