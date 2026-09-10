using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class BackgroundMigrationsClient(IGitLabApiConnection connection)
    : IBackgroundMigrationsClient
{
    public IAsyncEnumerable<GitLabBatchedBackgroundMigration> ListMigrationsAsync(
        BatchedBackgroundMigrationListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("admin").Literal("batched_background_migrations").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabBatchedBackgroundMigrationArray,
            cancellationToken);
    }

    public Task<GitLabBatchedBackgroundMigration> GetMigrationAsync(long id,
        BatchedBackgroundMigrationGetOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("admin").Literal("batched_background_migrations").Segment(id)
                .QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabBatchedBackgroundMigration,
            cancellationToken);
    }

    public Task<GitLabBatchedBackgroundMigration> PauseMigrationAsync(long id,
        BackgroundJobDatabaseRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("admin").Literal("batched_background_migrations").Segment(id)
                .Literal("pause").Build(),
            request ?? new BackgroundJobDatabaseRequest(),
            GitLabJsonContext.Default.BackgroundJobDatabaseRequest,
            GitLabJsonContext.Default.GitLabBatchedBackgroundMigration,
            cancellationToken);
    }

    public Task<GitLabBatchedBackgroundMigration> ResumeMigrationAsync(long id,
        BackgroundJobDatabaseRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("admin").Literal("batched_background_migrations").Segment(id)
                .Literal("resume").Build(),
            request ?? new BackgroundJobDatabaseRequest(),
            GitLabJsonContext.Default.BackgroundJobDatabaseRequest,
            GitLabJsonContext.Default.GitLabBatchedBackgroundMigration,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBatchedBackgroundOperation> ListOperationsAsync(
        BatchedBackgroundOperationListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("admin").Literal("batched_background_operations").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabBatchedBackgroundOperationArray,
            cancellationToken);
    }

    public Task<GitLabBatchedBackgroundOperation> GetOperationAsync(long id,
        BatchedBackgroundOperationGetOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("admin").Literal("batched_background_operations").Segment(id)
                .QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabBatchedBackgroundOperation,
            cancellationToken);
    }

    public Task<GitLabBatchedBackgroundOperation> RestartOperationAsync(long id,
        BackgroundJobDatabaseRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("admin").Literal("batched_background_operations").Segment(id)
                .Literal("restart").Build(),
            request ?? new BackgroundJobDatabaseRequest(),
            GitLabJsonContext.Default.BackgroundJobDatabaseRequest,
            GitLabJsonContext.Default.GitLabBatchedBackgroundOperation,
            cancellationToken);
    }

    public Task<GitLabBatchedBackgroundOperation> StopOperationAsync(long id,
        BackgroundJobDatabaseRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("admin").Literal("batched_background_operations").Segment(id)
                .Literal("stop").Build(),
            request ?? new BackgroundJobDatabaseRequest(),
            GitLabJsonContext.Default.BackgroundJobDatabaseRequest,
            GitLabJsonContext.Default.GitLabBatchedBackgroundOperation,
            cancellationToken);
    }
}