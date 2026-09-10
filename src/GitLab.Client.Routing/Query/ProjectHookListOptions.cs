using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Pagination controls for listing a project's webhooks
///     (<c>GET /projects/:id/hooks</c>).
///     <para>
///         The client continues through GitLab's <c>Link</c> headers after the selected starting page. Set
///         <see cref="Page" /> only to skip preceding pages, rather than to limit the stream to one page.
///     </para>
/// </summary>
[GitLabQuery]
public readonly record struct ProjectHookListOptions
{
    /// <summary>One-based page at which the asynchronous stream starts.</summary>
    public int? Page { get; init; }

    /// <summary>Maximum number of webhooks GitLab returns in each page.</summary>
    public int? PerPage { get; init; }
}