namespace GitLab.Client.Models.Requests;

/// <summary>The body of <c>POST /projects/:id/merge_requests/:merge_request_iid/context_commits</c>.</summary>
public sealed record CreateMergeRequestContextCommitsRequest
{
    /// <summary>The shas of the commits to attach to the merge request as context.</summary>
    public required IReadOnlyList<string> Commits { get; init; }
}