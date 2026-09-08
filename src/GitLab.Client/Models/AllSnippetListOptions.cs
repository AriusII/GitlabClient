using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for <c>GET /snippets/all</c>, the administrator/auditor feed over every snippet on the
///     instance. Separate from <see cref="SnippetListOptions" /> because only this endpoint honours
///     <see cref="RepositoryStorage" />.
/// </summary>
[GitLabQuery]
public readonly record struct AllSnippetListOptions
{
    /// <summary>Return only snippets created after this instant.</summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>Return only snippets created before this instant.</summary>
    public DateTimeOffset? CreatedBefore { get; init; }

    /// <summary>
    ///     Return only snippets whose repository lives on this Gitaly storage shard. Free text: shard names
    ///     are per-instance configuration.
    /// </summary>
    public string? RepositoryStorage { get; init; }

    public int? PerPage { get; init; }
}