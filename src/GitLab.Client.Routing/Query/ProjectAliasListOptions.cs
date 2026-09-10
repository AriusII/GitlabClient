using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Paging options for listing project aliases (<c>GET /project_aliases</c>), which takes no filters.</summary>
[GitLabQuery]
public readonly record struct ProjectAliasListOptions
{
    /// <summary>Page size. Pagination itself is followed automatically, so this only tunes the request count.</summary>
    public int? PerPage { get; init; }
}