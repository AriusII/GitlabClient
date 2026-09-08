using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for the two migration entity listings, <c>GET /bulk_imports/entities</c> and
///     <c>GET /bulk_imports/:import_id/entities</c>.
/// </summary>
/// <remarks>
///     <see cref="Sort" /> is declared only by the instance-wide listing. GitLab runs Grape, which
///     answers an unknown query parameter with a 200 and ignores it, so setting it on the per-migration
///     listing is harmless but has no effect.
/// </remarks>
[GitLabQuery]
public readonly record struct BulkImportEntityListOptions
{
    /// <summary>Return entities sorted by creation time. GitLab defaults to newest first.</summary>
    public GitLabBulkImportSort? Sort { get; init; }

    /// <summary>Return only entities in this lifecycle state.</summary>
    public GitLabBulkImportStatus? Status { get; init; }

    /// <summary>
    ///     The first page to fetch. Listing streams every following page on its own, so this skips the
    ///     pages before it rather than pinning the answer to a single page.
    /// </summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}