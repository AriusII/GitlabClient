using System.Net;
using System.Text;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class BackgroundMigrationsRepositoryTests
{
    private const string MigrationJson = """
                                         {
                                           "id": "1234",
                                           "job_class_name": "CopyColumnUsingBackgroundMigrationJob",
                                           "table_name": "events",
                                           "column_name": "id",
                                           "status": "active",
                                           "progress": 50,
                                           "created_at": "2022-11-28T16:26:39+02:00",
                                           "estimated_time_remaining": "1 day"
                                         }
                                         """;

    private const string OperationJson = """
                                         {
                                           "id": "<cluster>:1:42",
                                           "partition": 1,
                                           "job_class_name": "UsersDeleteUnconfirmedSecondaryEmails",
                                           "table_name": "users",
                                           "column_name": "id",
                                           "status": "active",
                                           "created_at": "2025-05-15T10:00:00Z",
                                           "started_at": "2025-05-15T10:05:00Z",
                                           "finished_at": null,
                                           "on_hold_until": null
                                         }
                                         """;

    [Fact]
    public async Task ListMigrationsAsync_AppliesDatabaseAndJobClassNameFilters_AndDeserializesTheArray()
    {
        string json = $"[{MigrationJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BackgroundMigrationsRepository repository = new(connection);

        BatchedBackgroundMigrationListOptions options = new()
        {
            Database = GitLabBackgroundJobDatabase.Ci, JobClassName = "CopyColumnUsingBackgroundMigrationJob"
        };

        List<GitLabBatchedBackgroundMigration> migrations = new();
        await foreach (GitLabBatchedBackgroundMigration item in
                       repository.ListMigrationsAsync(options, TestContext.Current.CancellationToken))
        {
            migrations.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/admin/batched_background_migrations"
            + "?database=ci&job_class_name=CopyColumnUsingBackgroundMigrationJob",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBatchedBackgroundMigration migration = Assert.Single(migrations);
        Assert.Equal("1234", migration.Id);
        Assert.Equal("events", migration.TableName);
        Assert.Equal("active", migration.Status);
        Assert.Equal(50, migration.Progress);
        Assert.Equal("1 day", migration.EstimatedTimeRemaining);
    }

    [Fact]
    public async Task GetMigrationAsync_BuildsIdRoute_WithDatabaseQuery()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(MigrationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BackgroundMigrationsRepository repository = new(connection);

        GitLabBatchedBackgroundMigration migration = await repository.GetMigrationAsync(1234,
            new BatchedBackgroundMigrationGetOptions { Database = GitLabBackgroundJobDatabase.Main },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/batched_background_migrations/1234?database=main",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("1234", migration.Id);
    }

    [Fact]
    public async Task PauseMigrationAsync_PutsTheDatabaseSelector()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(MigrationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BackgroundMigrationsRepository repository = new(connection);

        await repository.PauseMigrationAsync(1234,
            new BackgroundJobDatabaseRequest { Database = GitLabBackgroundJobDatabase.Embedding },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/batched_background_migrations/1234/pause",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"database":"embedding"}""", sentBody);
    }

    [Fact]
    public async Task ResumeMigrationAsync_WithNoRequest_SendsAnEmptyBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(MigrationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BackgroundMigrationsRepository repository = new(connection);

        await repository.ResumeMigrationAsync(1234, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/batched_background_migrations/1234/resume",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{}", sentBody);
    }

    [Fact]
    public async Task ListOperationsAsync_BuildsRoute_AndDeserializesTheArray()
    {
        string json = $"[{OperationJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BackgroundMigrationsRepository repository = new(connection);

        List<GitLabBatchedBackgroundOperation> operations = new();
        await foreach (GitLabBatchedBackgroundOperation item in
                       repository.ListOperationsAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            operations.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/admin/batched_background_operations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBatchedBackgroundOperation operation = Assert.Single(operations);
        Assert.Equal("<cluster>:1:42", operation.Id);
        Assert.Equal(1, operation.Partition);
        Assert.Equal("users", operation.TableName);
        Assert.Null(operation.FinishedAt);
    }

    [Fact]
    public async Task ListOperationsAsync_AppliesDatabaseAndJobClassNameFilters()
    {
        string json = $"[{OperationJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BackgroundMigrationsRepository repository = new(connection);

        BatchedBackgroundOperationListOptions options = new()
        {
            Database = GitLabBackgroundJobDatabase.Ci, JobClassName = "UsersDeleteUnconfirmedSecondaryEmails"
        };

        List<GitLabBatchedBackgroundOperation> operations = new();
        await foreach (GitLabBatchedBackgroundOperation item in
                       repository.ListOperationsAsync(options, TestContext.Current.CancellationToken))
        {
            operations.Add(item);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/admin/batched_background_operations"
            + "?database=ci&job_class_name=UsersDeleteUnconfirmedSecondaryEmails",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(operations);
    }

    [Fact]
    public async Task GetOperationAsync_BuildsIdRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(OperationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BackgroundMigrationsRepository repository = new(connection);

        GitLabBatchedBackgroundOperation operation =
            await repository.GetOperationAsync(42, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/admin/batched_background_operations/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("active", operation.Status);
    }

    [Fact]
    public async Task GetOperationAsync_BuildsIdRoute_WithDatabaseQuery()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(OperationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BackgroundMigrationsRepository repository = new(connection);

        GitLabBatchedBackgroundOperation operation = await repository.GetOperationAsync(42,
            new BatchedBackgroundOperationGetOptions { Database = GitLabBackgroundJobDatabase.Main },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/admin/batched_background_operations/42?database=main",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("<cluster>:1:42", operation.Id);
    }

    [Fact]
    public async Task RestartOperationAsync_PutsTheDatabaseSelector()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(OperationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BackgroundMigrationsRepository repository = new(connection);

        await repository.RestartOperationAsync(42,
            new BackgroundJobDatabaseRequest { Database = GitLabBackgroundJobDatabase.Sec },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/batched_background_operations/42/restart",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"database":"sec"}""", sentBody);
    }

    [Fact]
    public async Task StopOperationAsync_PutsTheDatabaseSelector()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(OperationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BackgroundMigrationsRepository repository = new(connection);

        await repository.StopOperationAsync(42,
            new BackgroundJobDatabaseRequest { Database = GitLabBackgroundJobDatabase.Geo },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/batched_background_operations/42/stop",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"database":"geo"}""", sentBody);
    }
}