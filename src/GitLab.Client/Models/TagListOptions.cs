using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing a project's tags (<c>GET /projects/:id/repository/tags</c>).</summary>
[GitLabQuery]
public readonly record struct TagListOptions
{
    public string? OrderBy { get; init; }

    public string? Sort { get; init; }

    public string? Search { get; init; }

    public int? PerPage { get; init; }
}