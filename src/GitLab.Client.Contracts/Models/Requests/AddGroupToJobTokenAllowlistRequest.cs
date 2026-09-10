namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/job_token_scope/groups_allowlist</c>.</summary>
public sealed record AddGroupToJobTokenAllowlistRequest
{
    /// <summary>The numeric id of the group to add to the allowlist.</summary>
    public required long TargetGroupId { get; init; }
}