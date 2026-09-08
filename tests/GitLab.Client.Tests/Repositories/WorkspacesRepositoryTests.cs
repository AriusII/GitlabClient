using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class WorkspacesRepositoryTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetAgentInfoAsync_BuildsTheInternalAgentwRoute_AndSurfacesTheRawPayload()
    {
        const string Json = """{ "workspace_id": 7, "state": "running" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        WorkspacesRepository repository = new(new GitLabApiConnection(httpClient));

        JsonElement result = await repository.GetAgentInfoAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/internal/agents/agentw/agent_info",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(7, result.GetProperty("workspace_id").GetInt32());
        Assert.Equal("running", result.GetProperty("state").GetString());
    }

    [Fact]
    public async Task AuthorizeUserAccessAsync_SendsTheWorkspaceHostAndUserIdAsQueryParameters()
    {
        const string Json = """{ "authorized": true }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        WorkspacesRepository repository = new(new GitLabApiConnection(httpClient));

        JsonElement result = await repository.AuthorizeUserAccessAsync("workspace-1.example.com", 42,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/internal/agents/agentw/authorize_user_access?workspace_host=workspace-1.example.com&user_id=42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.True(result.GetProperty("authorized").GetBoolean());
    }

    [Fact]
    public async Task AuthorizeUserAccessAsync_PercentEncodesTheWorkspaceHost()
    {
        const string Json = """{ "authorized": true }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        WorkspacesRepository repository = new(new GitLabApiConnection(httpClient));

        // A workspace agent host can carry a non-default port. ':' is not in Uri.EscapeDataString's
        // unreserved set, so it comes out as "%3A" - proving the free-text host actually goes through
        // Query()'s escaping rather than being appended raw.
        _ = await repository.AuthorizeUserAccessAsync("workspace-1.example.com:3443", 42,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/internal/agents/agentw/authorize_user_access" +
            "?workspace_host=workspace-1.example.com%3A3443&user_id=42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task AuthorizeUserAccessAsync_OnUnauthorized_ThrowsGitLabAuthenticationException()
    {
        const string Json = """{ "message": "401 Unauthorized" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        WorkspacesRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabAuthenticationException>(() =>
            repository.AuthorizeUserAccessAsync("workspace-1.example.com", 42,
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }
}