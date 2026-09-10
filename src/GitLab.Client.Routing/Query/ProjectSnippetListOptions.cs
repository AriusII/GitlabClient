using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Paging options for <c>GET /projects/:id/snippets</c>. The endpoint exposes no server-side
///     filters, but accepting <see cref="PerPage" /> lets callers tune the payload size while the client
///     continues to follow GitLab's <c>Link: rel="next"</c> cursor until the sequence is exhausted.
/// </summary>
[GitLabQuery]
public readonly record struct ProjectSnippetListOptions
{
    /// <summary>The maximum number of snippets GitLab should return in each fetched page.</summary>
    public int? PerPage { get; init; }
}