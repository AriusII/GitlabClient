using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Batched background migrations and Batched background operations
///     resource: builds routes via <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IBackgroundMigrationsService), typeof(IBackgroundMigrationsClient))]
internal interface IBackgroundMigrationsRepository
{
    IAsyncEnumerable<GitLabBatchedBackgroundMigration> ListMigrationsAsync(
        BatchedBackgroundMigrationListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabBatchedBackgroundMigration> GetMigrationAsync(long id,
        BatchedBackgroundMigrationGetOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabBatchedBackgroundMigration> PauseMigrationAsync(long id,
        BackgroundJobDatabaseRequest? request = null, CancellationToken cancellationToken = default);

    Task<GitLabBatchedBackgroundMigration> ResumeMigrationAsync(long id,
        BackgroundJobDatabaseRequest? request = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBatchedBackgroundOperation> ListOperationsAsync(
        BatchedBackgroundOperationListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabBatchedBackgroundOperation> GetOperationAsync(long id,
        BatchedBackgroundOperationGetOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabBatchedBackgroundOperation> RestartOperationAsync(long id,
        BackgroundJobDatabaseRequest? request = null, CancellationToken cancellationToken = default);

    Task<GitLabBatchedBackgroundOperation> StopOperationAsync(long id,
        BackgroundJobDatabaseRequest? request = null, CancellationToken cancellationToken = default);
}