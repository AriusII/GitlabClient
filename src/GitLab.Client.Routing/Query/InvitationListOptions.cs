using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for listing pending invitations (<c>GET /projects/:id/invitations</c>).</summary>
[GitLabQuery]
public readonly record struct InvitationListOptions
{
    /// <summary>
    ///     A member search term matched against the invited email address or user name. GitLab names this
    ///     parameter literally <c>query</c>; it is a search string, not a filter expression.
    /// </summary>
    public string? Query { get; init; }

    /// <summary>The one-based result page to retrieve.</summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}