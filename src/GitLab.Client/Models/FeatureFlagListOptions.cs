using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing a project's feature flags (<c>GET /projects/:id/feature_flags</c>).</summary>
[GitLabQuery]
public sealed record FeatureFlagListOptions
{
    /// <summary>Restricts the answer to enabled or to disabled flags. GitLab returns both when unset.</summary>
    public GitLabFeatureFlagState? Scope { get; init; }

    /// <summary>
    ///     The first page to fetch. Listing streams every following page on its own, so this skips the pages
    ///     before it rather than pinning the answer to a single page.
    /// </summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}