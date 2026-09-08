using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Paging and inclusion filters for <c>GET /projects/:id/registry/repositories</c>.</summary>
[GitLabQuery]
public sealed record RegistryRepositoryListOptions
{
    public int? Page { get; init; }

    public int? PerPage { get; init; }

    /// <summary>Include each repository's tags in the response.</summary>
    public bool? Tags { get; init; }

    /// <summary>Include each repository's tag count in the response.</summary>
    public bool? TagsCount { get; init; }
}