namespace GitLab.Client.Models;

/// <summary>Request body for <c>PATCH /projects/:id/job_token_scope</c>.</summary>
public sealed record UpdateProjectJobTokenScopeRequest
{
    /// <summary>
    ///     Whether CI/CD job tokens generated in other projects have restricted access to this project.
    /// </summary>
    public required bool Enabled { get; init; }
}