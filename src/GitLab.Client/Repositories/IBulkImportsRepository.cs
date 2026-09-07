using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the direct-transfer migration resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IBulkImportsService), typeof(IBulkImportsClient))]
internal interface IBulkImportsRepository
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