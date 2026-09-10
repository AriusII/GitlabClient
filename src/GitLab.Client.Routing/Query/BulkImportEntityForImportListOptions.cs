using GitLab.Client.Models;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for the entities of one direct-transfer migration
///     (<c>GET /bulk_imports/:import_id/entities</c>).
/// </summary>
/// <remarks>
///     This route deliberately does not expose <c>sort</c>: unlike the instance-wide entities route,
///     GitLab 19.x does not accept it. Keeping the query shape exact prevents Grape from returning a
///     successful response while silently ignoring a caller's requested ordering.
/// </remarks>
[GitLabQuery]
public readonly record struct BulkImportEntityForImportListOptions
{
    /// <summary>Return only entities in this lifecycle state.</summary>
    public GitLabBulkImportStatus? Status { get; init; }

    /// <summary>
    ///     The first page to fetch. Listing streams every following page on its own, so this skips the
    ///     pages before it rather than pinning the answer to a single page.
    /// </summary>
    public int? Page { get; init; }

    /// <summary>The maximum number of entities GitLab returns per page.</summary>
    public int? PerPage { get; init; }
}