namespace GitLab.Client.Models;

/// <summary>
///     How much a user account still owns, as reported by
///     <c>GET /users/:id/associations_count</c> - the check an administrator makes before deleting an
///     account, since a hard delete takes these with it.
/// </summary>
public sealed record GitLabUserAssociationsCount
{
    public int? GroupsCount { get; init; }

    public int? ProjectsCount { get; init; }

    public int? IssuesCount { get; init; }

    public int? MergeRequestsCount { get; init; }
}