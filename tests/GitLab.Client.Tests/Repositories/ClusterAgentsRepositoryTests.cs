using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ClusterAgentsRepositoryTests
{
    private const string BaseAddress = "https://gitlab.example/api/v4/";

    private const string ClusterAgentJson = """
                                            {
                                              "id": 1,
                                              "name": "k8s-agent",
                                              "config_project": {
                                                "id": 20,
                                                "description": "",
                                                "name": "kubernetes-agents",
                                                "name_with_namespace": "Ops / kubernetes-agents",
                                                "path": "kubernetes-agents",
                                                "path_with_namespace": "ops/kubernetes-agents",
                                                "created_at": "2025-01-15T09:00:00.000Z"
                                              },
                                              "created_at": "2025-01-20T10:30:00.000Z",
                                              "created_by_user_id": 42,
                                              "is_receptive": false
                                            }
                                            """;

    /// <summary>The listing shape (<c>APIEntitiesClustersAgentTokenBasic</c>): no <c>last_used_at</c>.</summary>
    private const string ClusterAgentTokenBasicJson = """
                                                      {
                                                        "id": 5,
                                                        "name": "token-1",
                                                        "description": "Production token",
                                                        "agent_id": 1,
                                                        "status": "active",
                                                        "created_at": "2025-02-01T08:00:00.000Z",
                                                        "created_by_user_id": 42
                                                      }
                                                      """;

    /// <summary>The single-token read shape (<c>APIEntitiesClustersAgentToken</c>): adds <c>last_used_at</c>.</summary>
    private const string ClusterAgentTokenJson = """
                                                 {
                                                   "id": 5,
                                                   "name": "token-1",
                                                   "description": "Production token",
                                                   "agent_id": 1,
                                                   "status": "active",
                                                   "created_at": "2025-02-01T08:00:00.000Z",
                                                   "created_by_user_id": 42,
                                                   "last_used_at": "2025-03-05T12:00:00.000Z"
                                                 }
                                                 """;

    private const string ClusterAgentUrlConfigurationJson = """
                                                            {
                                                              "id": 9,
                                                              "agent_id": 1,
                                                              "url": "grpcs://agent.example.com:443",
                                                              "public_key": "-----BEGIN PUBLIC KEY-----\nMIIB...\n-----END PUBLIC KEY-----",
                                                              "client_cert": "-----BEGIN CERTIFICATE-----\nMIIC...\n-----END CERTIFICATE-----",
                                                              "ca_cert": "-----BEGIN CERTIFICATE-----\nMIID...\n-----END CERTIFICATE-----",
                                                              "tls_host": "agent.example.com"
                                                            }
                                                            """;

    [Fact]
    public async Task ListAsync_BuildsProjectClusterAgentsRoute_AndDeserializesEachAgent()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{ClusterAgentJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        List<GitLabClusterAgent> agents = new();
        await foreach (GitLabClusterAgent agent in repository.ListAsync(ProjectId.FromId(5),
                           TestContext.Current.CancellationToken))
        {
            agents.Add(agent);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/cluster_agents",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabClusterAgent only = Assert.Single(agents);
        Assert.Equal(1, only.Id);
        Assert.Equal("k8s-agent", only.Name);
        Assert.Equal(new DateTimeOffset(2025, 1, 20, 10, 30, 0, TimeSpan.Zero), only.CreatedAt);
        Assert.Equal(42, only.CreatedByUserId);
        Assert.False(only.IsReceptive);

        Assert.Equal(20, only.ConfigProject?.Id);
        Assert.Equal("kubernetes-agents", only.ConfigProject?.Name);
        Assert.Equal("ops/kubernetes-agents", only.ConfigProject?.PathWithNamespace);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        await foreach (GitLabClusterAgent _ in repository.ListAsync(ProjectId.FromPath("gitlab-org/gitlab"),
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/cluster_agents",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_PostsToProjectClusterAgentsRoute_WithSerializedBody_AndDeserializesTheCreatedAgent()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ClusterAgentJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        GitLabClusterAgent agent = await repository.CreateAsync(5, new CreateClusterAgentRequest { Name = "k8s-agent" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/cluster_agents",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal("""{"name":"k8s-agent"}""", sentBody);
        Assert.Equal(1, agent.Id);
        Assert.Equal("k8s-agent", agent.Name);
    }

    [Fact]
    public async Task GetAsync_BuildsSingleAgentRoute_AndDeserializesTheAgent()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ClusterAgentJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        GitLabClusterAgent agent = await repository.GetAsync(5, 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/cluster_agents/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, agent.Id);
        Assert.Equal("k8s-agent", agent.Name);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToSingleAgentRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        await repository.DeleteAsync(5, 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/cluster_agents/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListTokensAsync_BuildsAgentTokensRoute_AndDeserializesTheBasicShape()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{ClusterAgentTokenBasicJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        List<GitLabClusterAgentToken> tokens = new();
        await foreach (GitLabClusterAgentToken token in repository.ListTokensAsync(5, 1,
                           TestContext.Current.CancellationToken))
        {
            tokens.Add(token);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/cluster_agents/1/tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabClusterAgentToken only = Assert.Single(tokens);
        Assert.Equal(5, only.Id);
        Assert.Equal("token-1", only.Name);
        Assert.Equal("Production token", only.Description);
        Assert.Equal(1, only.AgentId);
        Assert.Equal("active", only.Status);
        Assert.Equal(42, only.CreatedByUserId);

        // The listing shape carries no last_used_at at all.
        Assert.Null(only.LastUsedAt);
    }

    [Fact]
    public async Task CreateTokenAsync_PostsToAgentTokensRoute_WithSerializedBody_AndReturnsTheSecret()
    {
        const string Json = """
                            {
                              "id": 5,
                              "name": "token-1",
                              "description": "Production token",
                              "agent_id": 1,
                              "status": "active",
                              "created_at": "2025-02-01T08:00:00.000Z",
                              "created_by_user_id": 42,
                              "last_used_at": null,
                              "token": "glagent-8ETVzC1YMx4sJ4qkeQ2t"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        CreateClusterAgentTokenRequest request = new() { Name = "token-1", Description = "Production token" };

        GitLabClusterAgentTokenWithSecret token = await repository.CreateTokenAsync(5, 1, request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/cluster_agents/1/tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"token-1","description":"Production token"}""", sentBody);
        Assert.Equal(5, token.Id);
        Assert.Equal("glagent-8ETVzC1YMx4sJ4qkeQ2t", token.Token);
        Assert.Null(token.LastUsedAt);

        // The secret must never leak through the redacting ToString override.
        Assert.DoesNotContain("glagent-8ETVzC1YMx4sJ4qkeQ2t", token.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetTokenAsync_BuildsSingleTokenRoute_AndDeserializesTheFullShape()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ClusterAgentTokenJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        GitLabClusterAgentToken token = await repository.GetTokenAsync(5, 1, 5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/cluster_agents/1/tokens/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(5, token.Id);
        Assert.Equal(new DateTimeOffset(2025, 3, 5, 12, 0, 0, TimeSpan.Zero), token.LastUsedAt);
    }

    [Fact]
    public async Task RevokeTokenAsync_SendsDeleteToSingleTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        await repository.RevokeTokenAsync(5, 1, 5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/cluster_agents/1/tokens/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListUrlConfigurationsAsync_BuildsUrlConfigurationsRoute_AndDeserializesEachConfiguration()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{ClusterAgentUrlConfigurationJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        List<GitLabClusterAgentUrlConfiguration> configurations = new();
        await foreach (GitLabClusterAgentUrlConfiguration configuration in repository.ListUrlConfigurationsAsync(5, 1,
                           TestContext.Current.CancellationToken))
        {
            configurations.Add(configuration);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/cluster_agents/1/url_configurations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabClusterAgentUrlConfiguration only = Assert.Single(configurations);
        Assert.Equal(9, only.Id);
        Assert.Equal(1, only.AgentId);
        Assert.Equal(new Uri("grpcs://agent.example.com:443"), only.Url);
        Assert.Equal("agent.example.com", only.TlsHost);
        Assert.StartsWith("-----BEGIN PUBLIC KEY-----", only.PublicKey, StringComparison.Ordinal);
    }

    [Fact]
    public async Task
        CreateUrlConfigurationAsync_PostsToUrlConfigurationsRoute_WithSerializedBody_AndDeserializesTheConfiguration()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ClusterAgentUrlConfigurationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        CreateClusterAgentUrlConfigurationRequest request = new()
        {
            Url = new Uri("grpcs://agent.example.com:443"),
            ClientCert = "-----BEGIN CERTIFICATE-----",
            ClientKey = "-----BEGIN PRIVATE KEY-----",
            TlsHost = "agent.example.com"
        };

        GitLabClusterAgentUrlConfiguration configuration = await repository.CreateUrlConfigurationAsync(5, 1, request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/cluster_agents/1/url_configurations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"url\":\"grpcs://agent.example.com:443\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"client_key\":\"-----BEGIN PRIVATE KEY-----\"", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("ca_cert", sentBody, StringComparison.Ordinal);
        Assert.Equal(9, configuration.Id);
        Assert.Equal(new Uri("grpcs://agent.example.com:443"), configuration.Url);
    }

    [Fact]
    public async Task GetUrlConfigurationAsync_BuildsSingleUrlConfigurationRoute_AndDeserializesTheConfiguration()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ClusterAgentUrlConfigurationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        GitLabClusterAgentUrlConfiguration configuration = await repository.GetUrlConfigurationAsync(5, 1, 9,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/cluster_agents/1/url_configurations/9",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(9, configuration.Id);
        Assert.Equal("agent.example.com", configuration.TlsHost);
    }

    [Fact]
    public async Task DeleteUrlConfigurationAsync_SendsDeleteToSingleUrlConfigurationRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        await repository.DeleteUrlConfigurationAsync(5, 1, 9, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/cluster_agents/1/url_configurations/9",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnNotFoundResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Agent Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(5, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Agent Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_OnValidationError_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "name": ["has already been taken"] } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        ClusterAgentsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateAsync(5, new CreateClusterAgentRequest { Name = "duplicate" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }
}