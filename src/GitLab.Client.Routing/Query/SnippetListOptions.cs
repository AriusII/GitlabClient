using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for the two snippet feeds that take the same parameter set - <c>GET /snippets</c> (the
///     current user's own snippets) and <c>GET /snippets/public</c> (every public snippet).
/// </summary>
[GitLabQuery]
public readonly record struct SnippetListOptions
{
    /// <summary>Return only snippets created after this instant.</summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>Return only snippets created before this instant.</summary>
    public DateTimeOffset? CreatedBefore { get; init; }

    public int? PerPage { get; init; }
}