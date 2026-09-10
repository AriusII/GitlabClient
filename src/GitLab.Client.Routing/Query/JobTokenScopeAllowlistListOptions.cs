using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Paging for the CI/CD job token allowlist endpoints (<c>GET /projects/:id/job_token_scope/allowlist</c>
///     and <c>GET /projects/:id/job_token_scope/groups_allowlist</c>) - <c>page</c> and <c>per_page</c> are
///     the only query parameters either takes.
/// </summary>
[GitLabQuery]
public readonly record struct JobTokenScopeAllowlistListOptions
{
    /// <summary>
    ///     The first page to fetch. Listing streams every following page on its own, so this skips the pages
    ///     before it rather than pinning the answer to a single page.
    /// </summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}