using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing the pipelines that built a package
///     (<c>GET /projects/:id/packages/:package_id/pipelines</c>). Results are sorted by <c>id</c>
///     descending and capped at 20 per page.
/// </summary>
[GitLabQuery]
public readonly record struct PackagePipelineListOptions
{
    public int? Page { get; init; }

    public int? PerPage { get; init; }

    /// <summary>An opaque keyset cursor for resuming a listing; normally left unset.</summary>
    public string? Cursor { get; init; }
}