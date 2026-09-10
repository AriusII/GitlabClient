namespace GitLab.Client.Models.Requests;

/// <summary>The body of <c>POST /projects/:id/merge_requests/:merge_request_iid/time_estimate</c>.</summary>
public sealed record MergeRequestTimeEstimateRequest
{
    /// <summary>The estimate in GitLab's human duration format - "3h30m", "1w 2d".</summary>
    public required string Duration { get; init; }
}