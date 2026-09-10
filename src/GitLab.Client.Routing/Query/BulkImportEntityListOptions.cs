using GitLab.Client.Models;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for the instance-wide migration entity listing (<c>GET /bulk_imports/entities</c>).</summary>
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