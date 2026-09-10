using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's batched background migration and batched background operation admin APIs
///     (<c>/admin/batched_background_migrations</c>, <c>/admin/batched_background_operations</c>) -
///     the mechanisms Rails uses to run large data or schema changes in small batches over time rather
///     than as one blocking database transaction. Operations are the successor mechanism to
///     migrations, covering arbitrary background data changes rather than only schema migrations.
///     <para>
///         Every method here is available only to instance administrators; GitLab answers <c>403</c>
///         to anyone else.
///     </para>
/// </summary>
public interface IBackgroundMigrationsClient
{
    /// <summary>Streams every batched background migration on the instance.</summary>
    IAsyncEnumerable<GitLabBatchedBackgroundMigration> ListMigrationsAsync(
        BatchedBackgroundMigrationListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one batched background migration by id.</summary>
    Task<GitLabBatchedBackgroundMigration> GetMigrationAsync(long id,
        BatchedBackgroundMigrationGetOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Pauses an <c>active</c> batched background migration. GitLab answers <c>422</c> for any other status.</summary>
    Task<GitLabBatchedBackgroundMigration> PauseMigrationAsync(long id,
        BackgroundJobDatabaseRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Resumes a <c>paused</c> batched background migration. GitLab answers <c>422</c> for any other status.</summary>
    Task<GitLabBatchedBackgroundMigration> ResumeMigrationAsync(long id,
        BackgroundJobDatabaseRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Streams every batched background operation on the instance.</summary>
    IAsyncEnumerable<GitLabBatchedBackgroundOperation> ListOperationsAsync(
        BatchedBackgroundOperationListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one batched background operation by id.</summary>
    Task<GitLabBatchedBackgroundOperation> GetOperationAsync(long id,
        BatchedBackgroundOperationGetOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Restarts a <c>stopped</c> batched background operation. GitLab answers <c>422</c> for any other
    ///     status.
    /// </summary>
    Task<GitLabBatchedBackgroundOperation> RestartOperationAsync(long id,
        BackgroundJobDatabaseRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Stops a <c>queued</c>, <c>active</c> or <c>paused</c> batched background operation. GitLab
    ///     answers <c>422</c> for any other status.
    /// </summary>
    Task<GitLabBatchedBackgroundOperation> StopOperationAsync(long id,
        BackgroundJobDatabaseRequest? request = null, CancellationToken cancellationToken = default);
}