namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/issues/:issue_iid/time_estimate</c>.</summary>
public sealed record SetIssueTimeEstimateRequest
{
    /// <summary>The estimate in the human format GitLab parses - <c>3h30m</c>, <c>1d 2h</c>.</summary>
    public required string Duration { get; init; }
}