namespace GitLab.Client.Models;

/// <summary>
///     The count of recently created merge requests for a group, as returned by
///     <c>GET /analytics/group_activity/merge_requests_count</c>.
/// </summary>
public sealed record GitLabGroupMergeRequestsCount
{
    public required int MergeRequestsCount { get; init; }
}