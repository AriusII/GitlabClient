namespace GitLab.Client.Models;

/// <summary>
///     The count of members recently added to a group, as returned by
///     <c>GET /analytics/group_activity/new_members_count</c>.
/// </summary>
public sealed record GitLabGroupNewMembersCount
{
    public required int NewMembersCount { get; init; }
}