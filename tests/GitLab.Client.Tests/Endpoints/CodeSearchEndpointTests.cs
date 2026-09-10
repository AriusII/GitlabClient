using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class CodeSearchEndpointTests
{
    private const string IndexedNamespaceJson = """
                                                {
                                                  "id": 1234,
                                                  "zoekt_shard_id": 7,
                                                  "zoekt_node_id": 3,
                                                  "namespace_id": 9970,
                                                  "number_of_replicas_override": 2
                                                }
                                                """;

    [Fact]
    public async Task UpdateNamespaceReplicasAsync_UsesPatch_AndEscapesANamespacePathId()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(IndexedNamespaceJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CodeSearchClient repository = new(connection);

        GitLabZoektIndexedNamespace indexedNamespace = await repository.UpdateNamespaceReplicasAsync(
            "gitlab-org/gitlab",
            new UpdateZoektNamespaceReplicasRequest { NumberOfReplicasOverride = 2 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/zoekt/namespaces/gitlab-org%2Fgitlab",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"number_of_replicas_override":2}""", sentBody);

        Assert.Equal(2, indexedNamespace.NumberOfReplicasOverride);
        Assert.Equal(9970, indexedNamespace.NamespaceId);
    }

    [Fact]
    public async Task UpdateNamespaceReplicasAsync_WithNoRequest_SendsAnEmptyBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(IndexedNamespaceJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CodeSearchClient repository = new(connection);

        await repository.UpdateNamespaceReplicasAsync("42", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/admin/zoekt/namespaces/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{}", sentBody);
    }

    [Fact]
    public async Task IndexProjectAsync_PutsWithNoBody_AndDeserializesTheJobId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"job_id":"job-42"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CodeSearchClient repository = new(connection);

        GitLabZoektProjectIndexResult result =
            await repository.IndexProjectAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/zoekt/projects/7/index",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal("job-42", result.JobId);
    }

    [Fact]
    public async Task ListShardsAsync_GetsTheShardsRoute_AndDeserializesTheUnpaginatedArray()
    {
        const string Json = """
                            [
                              {
                                "id": 3,
                                "index_base_url": "http://127.0.0.1:6060/",
                                "search_base_url": "http://127.0.0.1:6070/"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CodeSearchClient repository = new(connection);

        IReadOnlyList<GitLabZoektNode> shards = await repository.ListShardsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/zoekt/shards", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabZoektNode shard = Assert.Single(shards);
        Assert.Equal(3, shard.Id);
        Assert.Equal(new Uri("http://127.0.0.1:6060/"), shard.IndexBaseUrl);
    }

    [Fact]
    public async Task ListIndexedNamespacesAsync_GetsTheNodesIndexedNamespacesRoute()
    {
        string json = $"[{IndexedNamespaceJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CodeSearchClient repository = new(connection);

        IReadOnlyList<GitLabZoektIndexedNamespace> namespaces =
            await repository.ListIndexedNamespacesAsync(3, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/admin/zoekt/shards/3/indexed_namespaces",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(7, Assert.Single(namespaces).ZoektShardId);
    }

    [Fact]
    public async Task RemoveIndexedNamespaceAsync_SendsDeleteToTheNodeAndNamespaceRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CodeSearchClient repository = new(connection);

        await repository.RemoveIndexedNamespaceAsync(3, 9970, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/zoekt/shards/3/indexed_namespaces/9970",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task AddIndexedNamespaceAsync_PutsTheSearchFlag()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(IndexedNamespaceJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CodeSearchClient repository = new(connection);

        GitLabZoektIndexedNamespace indexedNamespace = await repository.AddIndexedNamespaceAsync(3, 9970,
            new AddZoektIndexedNamespaceRequest { Search = true }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/zoekt/shards/3/indexed_namespaces/9970",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"search":true}""", sentBody);
        Assert.Equal(9970, indexedNamespace.NamespaceId);
    }

    [Fact]
    public async Task IndexProjectAsync_OnMissingProject_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CodeSearchClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.IndexProjectAsync(999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}