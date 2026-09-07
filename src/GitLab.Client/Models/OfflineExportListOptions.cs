using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for <c>GET /offline_exports</c>.</summary>
[GitLabQuery]
public sealed record OfflineExportListOptions
{
    /// <summary>Return exports sorted by creation time. GitLab defaults to newest first.</summary>
    public GitLabBulkImportSort? Sort { get; init; }

    /// <summary>Return only exports in this lifecycle state.</summary>
    public GitLabOfflineExportStatus? Status { get; init; }

    public int? PerPage { get; init; }
}