namespace GitLab.Client.Models;

/// <summary>
///     GitLab's acknowledgement of a posted status
///     (<c>POST /projects/:id/merge_requests/:iid/status_check_responses</c>): the recorded response, the merge
///     request it applies to, and a summary of the check that produced it.
/// </summary>
public sealed record GitLabStatusCheckResponse
{
    public required long Id { get; init; }

    public GitLabMergeRequest? MergeRequest { get; init; }

    public GitLabStatusCheckSummary? ExternalStatusCheck { get; init; }
}