namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/job_token_scope/allowlist</c>.</summary>
public sealed record AddProjectToJobTokenAllowlistRequest
{
    /// <summary>The numeric id of the project to add to the allowlist.</summary>
    public required long TargetProjectId { get; init; }
}