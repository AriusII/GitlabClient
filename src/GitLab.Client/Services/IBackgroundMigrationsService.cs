using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Batched background migrations and Batched background
///     operations, sitting between the public <c>IBackgroundMigrationsClient</c> controller and
///     <c>IBackgroundMigrationsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IBackgroundMigrationsService
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