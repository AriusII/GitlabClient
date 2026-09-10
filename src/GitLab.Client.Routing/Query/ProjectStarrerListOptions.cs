using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for the users who starred a project (<c>GET /projects/:id/starrers</c>).</summary>
[GitLabQuery]
public readonly record struct ProjectStarrerListOptions
{
    /// <summary>Return only users matching this search term.</summary>
    public string? Search { get; init; }

    public int? PerPage { get; init; }
}