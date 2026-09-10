namespace GitLab.Client.Models;

/// <summary>
///     The answer to <c>GET /projects/:id/search/semantic</c>: natural-language code search results,
///     grouped by file.
/// </summary>
public sealed record GitLabSemanticCodeSearchResult
{
    /// <summary>
    ///     Overall confidence in the result set - GitLab's description names "high", "medium", "low" and
    ///     "unknown" but the spec does not constrain the field to that list, so it is kept as a plain
    ///     <see cref="string" /> rather than a closed enum.
    /// </summary>
    public string? Confidence { get; init; }

    public IReadOnlyList<GitLabSemanticCodeSearchMatch>? Results { get; init; }
}