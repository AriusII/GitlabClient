using GitLab.Client.Models;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for the service account listings (<c>GET /service_accounts</c>,
///     <c>GET /groups/:id/service_accounts</c>, <c>GET /projects/:id/service_accounts</c>).
///     <para>
///         GitLab also declares a <c>page</c> parameter here; it is deliberately not exposed, because the
///         listings stream every page by following the <c>Link: rel="next"</c> header.
///     </para>
/// </summary>
[GitLabQuery]
public readonly record struct ServiceAccountListOptions
{
    /// <summary>Attribute to sort by - the account id or its username.</summary>
    public GitLabServiceAccountOrderBy? OrderBy { get; init; }

    /// <summary>Sort direction. GitLab defaults to descending.</summary>
    public GitLabServiceAccountSort? Sort { get; init; }

    public int? PerPage { get; init; }
}