using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for <c>GET /projects/:id/search/semantic</c>. The natural-language query itself
///     (<c>q</c>) is required and so is not here - it is a plain method parameter, matching how
///     <c>search</c> is handled on <see cref="SearchListOptions" />'s sibling routes.
/// </summary>
[GitLabQuery]
public sealed record SemanticCodeSearchOptions
{
    /// <summary>
    ///     Restrict the search to files under this directory (for example <c>"app/services/"</c>). Must be
    ///     relative - no leading slash, no <c>..</c> segments.
    /// </summary>
    public string? DirectoryPath { get; init; }

    /// <summary>Number of nearest neighbours to retrieve internally (GitLab default: 64).</summary>
    public int? Knn { get; init; }

    public int? Limit { get; init; }
}