using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class KnowledgeGraphRepositoryTests
{
    [Fact]
    public async Task ListNamespacesAsync_GetsTheNamespacesRoute_AndDeserializesTheUnpaginatedArray()
    {
        const string Json = """
                            [
                              {
                                "id": 1234,
                                "root_namespace_id": 5678,
                                "created_at": "2025-01-01T00:00:00Z"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        KnowledgeGraphRepository repository = new(connection);

        IReadOnlyList<GitLabKnowledgeGraphNamespace> namespaces =
            await repository.ListNamespacesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/knowledge_graph/namespaces",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabKnowledgeGraphNamespace enabledNamespace = Assert.Single(namespaces);
        Assert.Equal(1234, enabledNamespace.Id);
        Assert.Equal(5678, enabledNamespace.RootNamespaceId);
        Assert.Equal(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), enabledNamespace.CreatedAt);
    }

    [Fact]
    public async Task EnableNamespaceAsync_PutsWithNoBody_AndEscapesANamespacePathId()
    {
        const string Json = """{"id":1234,"root_namespace_id":5678,"created_at":"2025-01-01T00:00:00Z"}""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        KnowledgeGraphRepository repository = new(connection);

        GitLabKnowledgeGraphNamespace enabledNamespace =
            await repository.EnableNamespaceAsync("group/subgroup", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/knowledge_graph/namespaces/group%2Fsubgroup",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal(1234, enabledNamespace.Id);
    }

    [Fact]
    public async Task DisableNamespaceAsync_SendsDeleteToTheEscapedNamespaceRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        KnowledgeGraphRepository repository = new(connection);

        await repository.DisableNamespaceAsync("9970", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/knowledge_graph/namespaces/9970",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DisableNamespaceAsync_PercentEncodesANamespacePathId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        KnowledgeGraphRepository repository = new(connection);

        // Unlike the numeric-id case above, a namespace addressed by its full path must survive as one
        // path segment - the "/" has to come through as "%2F" or GitLab 404s against a route that
        // doesn't exist.
        await repository.DisableNamespaceAsync("group/subgroup", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/knowledge_graph/namespaces/group%2Fsubgroup",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task EnableNamespaceAsync_OnForbidden_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        KnowledgeGraphRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.EnableNamespaceAsync("9970", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }
}