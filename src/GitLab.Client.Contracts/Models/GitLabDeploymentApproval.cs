namespace GitLab.Client.Models;

/// <summary>
///     One approval or rejection recorded against a deployment to a protected environment, as returned by
///     <c>POST /projects/:id/deployments/:deployment_id/approval</c>.
/// </summary>
public sealed record GitLabDeploymentApproval
{
    /// <summary>The user who approved or rejected the deployment.</summary>
    public GitLabUser? User { get; init; }

    /// <summary>Either "approved" or "rejected".</summary>
    public required string Status { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? Comment { get; init; }
}