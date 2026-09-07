namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/deployments/:deployment_id</c>. The status is the only field the
///     endpoint accepts.
/// </summary>
public sealed record UpdateDeploymentRequest
{
    /// <summary>The new status: "running", "success", "failed" or "canceled".</summary>
    public required string Status { get; init; }
}