using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for <c>GET /bulk_imports</c>.</summary>
[GitLabQuery]
public sealed record BulkImportListOptions
{
    /// <summary>Return migrations sorted by creation time. GitLab defaults to newest first.</summary>
    public GitLabBulkImportSort? Sort { get; init; }

    /// <summary>Return only migrations in this lifecycle state.</summary>
    public GitLabBulkImportStatus? Status { get; init; }

    public int? PerPage { get; init; }
}