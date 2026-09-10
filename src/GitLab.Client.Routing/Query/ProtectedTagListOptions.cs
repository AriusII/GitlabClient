using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Paging options for a project's protected tags
///     (<c>GET /projects/:id/protected_tags</c>).
/// </summary>
[GitLabQuery]
public readonly record struct ProtectedTagListOptions
{
    /// <summary>
    ///     First offset-based page to fetch. The returned sequence follows GitLab's subsequent-page links,
    ///     so it skips earlier pages rather than restricting enumeration to one page.
    /// </summary>
    public int? Page { get; init; }

    /// <summary>Number of protected tags GitLab returns in each page.</summary>
    public int? PerPage { get; init; }
}