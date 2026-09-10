namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/deployments</c>. GitLab marks all five fields required.</summary>
public sealed record CreateDeploymentRequest
{
    /// <summary>The name of the environment to create the deployment for.</summary>
    public required string Environment { get; init; }

    /// <summary>The SHA of the commit that is deployed.</summary>
    public required string Sha { get; init; }

    /// <summary>The name of the branch or tag that is deployed.</summary>
    public required string Ref { get; init; }

    /// <summary>Whether the deployed ref is a tag (<see langword="true" />) or a branch (<see langword="false" />).</summary>
    public required bool Tag { get; init; }

    /// <summary>The status of the deployment being recorded: "running", "success", "failed" or "canceled".</summary>
    public required string Status { get; init; }
}