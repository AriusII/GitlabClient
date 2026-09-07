namespace GitLab.Client.Models;

/// <summary>
///     The count of recently created issues for a group, as returned by
///     <c>GET /analytics/group_activity/issues_count</c>.
/// </summary>
public sealed record GitLabGroupIssuesCount
{
    public required int IssuesCount { get; init; }
}