using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class AdminMigrationsRepositoryTests
{
    [Fact]
    public async Task ListPendingAsync_AppliesTheDatabaseFilter_AndReturnsTheRawPayload()
    {
        const string Json = """[{ "version": 20220101000000, "name": "AddIndexToUsers" }]""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AdminMigrationsRepository repository = new(connection);

        JsonElement result = await repository.ListPendingAsync(
            new AdminMigrationListOptions { Database = GitLabBackgroundJobDatabase.Ci },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/migrations/pending?database=ci",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(JsonValueKind.Array, result.ValueKind);
        Assert.Equal("AddIndexToUsers", result[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task ListPendingAsync_WithNoOptions_BuildsTheRouteWithoutAQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AdminMigrationsRepository repository = new(connection);

        await repository.ListPendingAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/admin/migrations/pending",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task MarkAppliedAsync_PostsTheDatabaseSelector_ToTheTimestampRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AdminMigrationsRepository repository = new(connection);

        await repository.MarkAppliedAsync(20220101000000,
            new BackgroundJobDatabaseRequest { Database = GitLabBackgroundJobDatabase.Main },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/migrations/20220101000000/mark",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"database":"main"}""", sentBody);
    }

    [Fact]
    public async Task MarkAppliedAsync_WithNoRequest_SendsAnEmptyBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AdminMigrationsRepository repository = new(connection);

        await repository.MarkAppliedAsync(20220101000000, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("{}", sentBody);
    }
}