using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for listing a project's feature flag user lists
///     (<c>GET /projects/:id/feature_flags_user_lists</c>).
/// </summary>
[GitLabQuery]
public readonly record struct FeatureFlagUserListOptions
{
    /// <summary>Return only user lists whose name matches this text.</summary>
    public string? Search { get; init; }

    /// <summary>
    ///     The first page to fetch. Listing streams every following page on its own, so this skips the pages
    ///     before it rather than pinning the answer to a single page.
    /// </summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}