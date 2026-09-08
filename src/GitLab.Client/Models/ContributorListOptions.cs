using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for contributor metrics (<c>GET /projects/:id/repository/contributors</c>).</summary>
[GitLabQuery]
public readonly record struct ContributorListOptions
{
    /// <summary>Branch or tag to count commits on. Defaults to the project's default branch.</summary>
    public string? Ref { get; init; }

    /// <summary>"email", "name" or "commits".</summary>
    public string? OrderBy { get; init; }

    /// <summary>"asc" or "desc".</summary>
    public string? Sort { get; init; }

    public int? PerPage { get; init; }
}