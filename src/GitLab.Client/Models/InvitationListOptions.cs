using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing pending invitations (<c>GET /projects/:id/invitations</c>).</summary>
[GitLabQuery]
public sealed record InvitationListOptions
{
    /// <summary>
    ///     A member search term matched against the invited email address or user name. GitLab names this
    ///     parameter literally <c>query</c>; it is a search string, not a filter expression.
    /// </summary>
    public string? Query { get; init; }

    public int? PerPage { get; init; }
}