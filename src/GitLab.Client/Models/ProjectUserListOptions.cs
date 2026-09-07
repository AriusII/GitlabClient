using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for the users of a project (<c>GET /projects/:id/users</c>).</summary>
[GitLabQuery]
public sealed record ProjectUserListOptions
{
    /// <summary>Return only users matching this search term.</summary>
    public string? Search { get; init; }

    /// <summary>Leave these user IDs out of the result.</summary>
    public IReadOnlyList<long>? SkipUsers { get; init; }

    public int? PerPage { get; init; }
}