using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ActiveContextRepositoryTests
{
    [Fact]
    public async Task UpdateEnabledNamespaceStateAsync_PutsToTheEnabledNamespacesRoute_AndSendsTheRawNamespaceId()
    {
        const string Json = """
                            {
                              "id": 1,
                              "namespace_id": 9970,
                              "connection_id": 1234,
                              "state": "pending",
                              "created_at": "2023-01-01T00:00:00.000Z",
                              "updated_at": "2023-01-01T00:00:00.000Z"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ActiveContextRepository repository = new(connection);

        using JsonDocument namespaceId = JsonDocument.Parse("9970");

        GitLabActiveContextCodeEnabledNamespace enabledNamespace = await repository.UpdateEnabledNamespaceStateAsync(
            new UpdateActiveContextEnabledNamespaceStateRequest
            {
                NamespaceId = namespaceId.RootElement.Clone(),
                State = GitLabActiveContextNamespaceState.Pending,
                ConnectionId = 1234
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/active_context/code/enabled_namespaces",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"namespace_id":9970,"state":"pending","connection_id":1234}""", sentBody);

        Assert.Equal(9970, enabledNamespace.NamespaceId);
        Assert.Equal("pending", enabledNamespace.State);
    }

    [Fact]
    public async Task UpdateEnabledNamespaceStateAsync_SendsANamespacePathAsAJsonString()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"id":1,"state":"ready"}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ActiveContextRepository repository = new(connection);

        using JsonDocument namespaceId = JsonDocument.Parse("\"gitlab-org/gitlab\"");

        await repository.UpdateEnabledNamespaceStateAsync(
            new UpdateActiveContextEnabledNamespaceStateRequest
            {
                NamespaceId = namespaceId.RootElement.Clone(), State = GitLabActiveContextNamespaceState.Ready
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("""{"namespace_id":"gitlab-org/gitlab","state":"ready"}""", sentBody);
    }

    [Fact]
    public async Task UpdateCollectionAsync_EscapesTheCollectionId_AndSendsOnlyTheGivenOptions()
    {
        const string Json = """
                            {
                              "id": 1,
                              "name": "gitlab_active_context_code",
                              "connection_id": 1234,
                              "options": { "queue_shard_count": 24, "queue_shard_limit": 1000 },
                              "created_at": "2023-01-01T00:00:00.000Z",
                              "updated_at": "2023-01-01T00:00:00.000Z"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ActiveContextRepository repository = new(connection);

        GitLabActiveContextCollectionDetail collection = await repository.UpdateCollectionAsync(
            "team/collection",
            new UpdateActiveContextCollectionRequest { QueueShardCount = 24 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/active_context/collections/team%2Fcollection",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"queue_shard_count":24}""", sentBody);

        Assert.Equal("gitlab_active_context_code", collection.Name);
        Assert.Equal(24, collection.Options!.Value.GetProperty("queue_shard_count").GetInt32());
    }

    [Fact]
    public async Task ListConnectionsAsync_GetsTheConnectionsRoute_AndDeserializesTheUnpaginatedArray()
    {
        const string Json = """
                            [
                              {
                                "id": 1234,
                                "name": "elastic",
                                "adapter_class": "ActiveContext::Databases::Elasticsearch::Adapter",
                                "prefix": "gitlab",
                                "active": true,
                                "created_at": "2023-01-01T00:00:00.000Z",
                                "updated_at": "2023-01-01T00:00:00.000Z"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ActiveContextRepository repository = new(connection);

        IReadOnlyList<GitLabActiveContextConnection> connections =
            await repository.ListConnectionsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/active_context/connections",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabActiveContextConnection connection1 = Assert.Single(connections);
        Assert.Equal(1234, connection1.Id);
        Assert.Equal("elastic", connection1.Name);
        Assert.True(connection1.Active);
    }

    [Fact]
    public async Task ActivateConnectionAsync_PutsTheConnectionId()
    {
        const string Json = """{"id":1234,"name":"elastic","active":true}""";

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ActiveContextRepository repository = new(connection);

        GitLabActiveContextConnection activated = await repository.ActivateConnectionAsync(
            new ActivateActiveContextConnectionRequest { ConnectionId = 1234 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/active_context/connections/activate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"connection_id":1234}""", sentBody);
        Assert.True(activated.Active);
    }

    [Fact]
    public async Task DeactivateConnectionAsync_WithNoRequest_SendsAnEmptyBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"id":1234,"active":false}""", Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ActiveContextRepository repository = new(connection);

        GitLabActiveContextConnection deactivated =
            await repository.DeactivateConnectionAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/active_context/connections/deactivate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{}", sentBody);
        Assert.False(deactivated.Active);
    }

    [Fact]
    public async Task ClearDeadQueueAsync_SendsDeleteToTheDeadQueueRoute()
    {
        // GitLab answers 200 here, not the usual 204 - DeleteAsync only checks for a success status
        // and never reads a body, so this still works.
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"cleared":3}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ActiveContextRepository repository = new(connection);

        await repository.ClearDeadQueueAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/active_context/dead_queue",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ReplayDeadQueueAsync_PostsTheTargetQueueName()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ActiveContextRepository repository = new(connection);

        await repository.ReplayDeadQueueAsync(new ReplayActiveContextDeadQueueRequest { Queue = "retry_queue" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/active_context/dead_queue/replay",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"queue":"retry_queue"}""", sentBody);
    }

    [Fact]
    public async Task ActivateConnectionAsync_OnForbidden_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ActiveContextRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.ActivateConnectionAsync(new ActivateActiveContextConnectionRequest { ConnectionId = 1 },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }
}