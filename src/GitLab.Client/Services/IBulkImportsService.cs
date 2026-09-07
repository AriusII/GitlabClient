using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for direct-transfer migrations, sitting between the public
///     <c>IBulkImportsClient</c> controller and <c>IBulkImportsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IBulkImportsService
{
    IAsyncEnumerable<GitLabBulkImport> ListAsync(BulkImportListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabBulkImport> CreateAsync(CreateBulkImportRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabBulkImport> GetAsync(long importId, CancellationToken cancellationToken = default);

    Task<GitLabBulkImport> CancelAsync(long importId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBulkImportEntity> ListEntitiesAsync(BulkImportEntityListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBulkImportEntity> ListEntitiesForImportAsync(long importId,
        BulkImportEntityListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabBulkImportEntity> GetEntityAsync(long importId, long entityId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBulkImportEntityFailure> ListEntityFailuresAsync(long importId, long entityId,
        CancellationToken cancellationToken = default);

    Task ImportGitHubGistsAsync(ImportGitHubGistsRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabOfflineExport> ListOfflineExportsAsync(OfflineExportListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task CreateOfflineExportAsync(CreateOfflineExportRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabOfflineExport> GetOfflineExportAsync(long exportId, CancellationToken cancellationToken = default);

    Task<GitLabBulkImport> CreateOfflineImportAsync(CreateOfflineImportRequest request,
        CancellationToken cancellationToken = default);
}