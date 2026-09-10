using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class BulkImportsClient(IGitLabApiConnection connection) : IBulkImportsClient
{
    public IAsyncEnumerable<GitLabBulkImport> ListAsync(BulkImportListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("bulk_imports").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabBulkImportArray,
            cancellationToken);
    }

    public Task<GitLabBulkImport> CreateAsync(CreateBulkImportRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostUrlEncodedFormAsync(
            GitLabRouteBuilder.Create("bulk_imports").Build(),
            request,
            GitLabJsonContext.Default.CreateBulkImportRequest,
            GitLabJsonContext.Default.GitLabBulkImport,
            cancellationToken);
    }

    public Task<GitLabBulkImport> GetAsync(long importId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("bulk_imports").Segment(importId).Build(),
            GitLabJsonContext.Default.GitLabBulkImport,
            cancellationToken);
    }

    public Task<GitLabBulkImport> CancelAsync(long importId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("bulk_imports").Segment(importId).Literal("cancel").Build(),
            GitLabJsonContext.Default.GitLabBulkImport,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBulkImportEntity> ListEntitiesAsync(BulkImportEntityListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("bulk_imports").Literal("entities").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabBulkImportEntityArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBulkImportEntity> ListEntitiesForImportAsync(long importId,
        BulkImportEntityForImportListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("bulk_imports").Segment(importId).Literal("entities").QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabBulkImportEntityArray,
            cancellationToken);
    }

    public Task<GitLabBulkImportEntity> GetEntityAsync(long importId, long entityId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("bulk_imports").Segment(importId).Literal("entities").Segment(entityId)
                .Build(),
            GitLabJsonContext.Default.GitLabBulkImportEntity,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBulkImportEntityFailure> ListEntityFailuresAsync(long importId, long entityId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("bulk_imports").Segment(importId).Literal("entities").Segment(entityId)
                .Literal("failures").Build(),
            GitLabJsonContext.Default.GitLabBulkImportEntityFailureArray,
            cancellationToken);
    }

    public Task ImportGitHubGistsAsync(ImportGitHubGistsRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("import").Literal("github").Literal("gists").Build(),
            request,
            GitLabJsonContext.Default.ImportGitHubGistsRequest,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabOfflineExport> ListOfflineExportsAsync(OfflineExportListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("offline_exports").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabOfflineExportArray,
            cancellationToken);
    }

    public Task CreateOfflineExportAsync(CreateOfflineExportRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("offline_exports").Build(),
            request,
            GitLabJsonContext.Default.CreateOfflineExportRequest,
            cancellationToken);
    }

    public Task<GitLabOfflineExport> GetOfflineExportAsync(long exportId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("offline_exports").Segment(exportId).Build(),
            GitLabJsonContext.Default.GitLabOfflineExport,
            cancellationToken);
    }

    public Task<GitLabBulkImport> CreateOfflineImportAsync(CreateOfflineImportRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("offline_imports").Build(),
            request,
            GitLabJsonContext.Default.CreateOfflineImportRequest,
            GitLabJsonContext.Default.GitLabBulkImport,
            cancellationToken);
    }
}