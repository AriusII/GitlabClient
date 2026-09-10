using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for listing a project's tags (<c>GET /projects/:id/repository/tags</c>).</summary>
[GitLabQuery]
public readonly record struct TagListOptions
{
    /// <summary>
    ///     Sort direction when <see cref="OrderBy" /> is <c>updated</c>: <c>asc</c> or <c>desc</c>.
    ///     GitLab defaults to <c>desc</c>.
    /// </summary>
    public string? Sort { get; init; }

    /// <summary>
    ///     Field used to order tags: <c>name</c>, <c>updated</c>, or <c>version</c>. GitLab defaults to
    ///     <c>updated</c>.
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>Search expression applied to the tag name.</summary>
    public string? Search { get; init; }

    /// <summary>
    ///     Tag name from which GitLab starts keyset pagination. This is distinct from <see cref="Page" />:
    ///     use one pagination strategy per request.
    /// </summary>
    public string? PageToken { get; init; }

    /// <summary>
    ///     First offset-based page to fetch. The returned sequence follows GitLab's subsequent-page links,
    ///     so it skips earlier pages rather than restricting enumeration to one page.
    /// </summary>
    public int? Page { get; init; }

    /// <summary>Number of tags GitLab returns in each page.</summary>
    public int? PerPage { get; init; }
}