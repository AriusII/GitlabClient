using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class InternalRepositoryTests
{
    private static StubHttpMessageHandler RespondWith(string json)
    {
        return new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }

    [Fact]
    public async Task ListGitalyObjectPoolMembersAsync_SendsTheCommaJoinedDiskPaths_AndReturnsTheRawJson()
    {
        const string Json = """[ { "relative_path": "@pools/ab/cd/pool.git" } ]""";

        using StubHttpMessageHandler handler = RespondWith(Json);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InternalRepository repository = new(connection);

        JsonElement members = await repository.ListGitalyObjectPoolMembersAsync(
            ["@pools/ab/cd/pool.git", "@hashed/12/34/repo.git"], "default", true,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/internal/gitaly/object_pool_members"
            + "?disk_paths=%40pools%2Fab%2Fcd%2Fpool.git,%40hashed%2F12%2F34%2Frepo.git"
            + "&storage=default&upstream_only=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(JsonValueKind.Array, members.ValueKind);
    }

    [Fact]
    public async Task GetSwaggerDocumentationAsync_WithoutAName_HitsTheRootRoute()
    {
        const string Json = """{ "swagger": "2.0" }""";

        using StubHttpMessageHandler handler = RespondWith(Json);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InternalRepository repository = new(connection);

        JsonElement doc = await repository.GetSwaggerDocumentationAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/swagger_doc", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("2.0", doc.GetProperty("swagger").GetString());
    }

    [Fact]
    public async Task GetSwaggerDocumentationAsync_WithAName_EscapesTheNameAndSendsTheLocale()
    {
        const string Json = """{ "swagger": "2.0" }""";

        using StubHttpMessageHandler handler = RespondWith(Json);

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InternalRepository repository = new(connection);

        JsonElement doc = await repository.GetSwaggerDocumentationAsync("v4/metadata", "en",
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/swagger_doc/v4%2Fmetadata?locale=en",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("2.0", doc.GetProperty("swagger").GetString());
    }

    [Fact]
    public async Task ListGitalyObjectPoolMembersAsync_OnBadRequest_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": "storage is missing" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InternalRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.ListGitalyObjectPoolMembersAsync(["a"], "", null,
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }
}