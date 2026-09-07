namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/deployments/:deployment_id/approval</c>.</summary>
public sealed record ApproveDeploymentRequest
{
    /// <summary>Either "approved" or "rejected".</summary>
    public required string Status { get; init; }

    /// <summary>An optional comment recorded alongside the decision.</summary>
    public string? Comment { get; init; }

    /// <summary>
    ///     The name of the user, group or role to approve as. Only meaningful - and demanded by GitLab, which
    ///     answers 400 without it - when the approver matches more than one of the protected environment's
    ///     approval rules.
    /// </summary>
    public string? RepresentedAs { get; init; }
}