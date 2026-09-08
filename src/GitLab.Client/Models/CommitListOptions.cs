using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing a project's commits (<c>GET /projects/:id/repository/commits</c>).</summary>
[GitLabQuery]
public readonly record struct CommitListOptions
{
    public string? RefName { get; init; }

    public DateTimeOffset? Since { get; init; }

    public DateTimeOffset? Until { get; init; }

    public int? PerPage { get; init; }
}