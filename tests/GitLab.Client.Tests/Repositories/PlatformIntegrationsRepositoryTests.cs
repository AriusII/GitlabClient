using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PlatformIntegrationsRepositoryTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetAllowedAgentsAsync_RequestsTheAllowedAgentsRoute_AndDeserializesTheJob()
    {
        const string Json = """
                            {
                              "id": 91,
                              "status": "running",
                              "name": "deploy",
                              "web_url": "https://gitlab.example/proj/-/jobs/91"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PlatformIntegrationsRepository repository = new(connection);

        GitLabJob job = await repository.GetAllowedAgentsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/job/allowed_agents", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(91, job.Id);
        Assert.Equal("running", job.Status);
    }

    [Fact]
    public async Task GetIamUserInfoAsync_RequestsTheUserinfoRoute_AndReturnsTheRawClaims()
    {
        const string Json = """{ "sub": "42", "email": "octo@example.com" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PlatformIntegrationsRepository repository = new(connection);

        JsonElement claims = await repository.GetIamUserInfoAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/iam/userinfo", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("42", claims.GetProperty("sub").GetString());
    }

    [Fact]
    public async Task GetGoogleCloudIntegrationSetupScriptAsync_BuildsTheRoute_WithTheProvidedOptions()
    {
        const string Script = "#!/bin/sh\necho setup\n";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Script, Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PlatformIntegrationsRepository repository = new(connection);

        GoogleCloudIntegrationSetupScriptOptions options = new()
        {
            EnableGoogleCloudArtifactRegistry = true, GoogleCloudArtifactRegistryProjectId = "my-gcp-project"
        };

        using GitLabFileResponse response = await repository.GetGoogleCloudIntegrationSetupScriptAsync(
            5, options, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/5/google_cloud/setup/integrations.sh"
            + "?enable_google_cloud_artifact_registry=true&google_cloud_artifact_registry_project_id=my-gcp-project",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("text/plain", response.ContentType);

        using StreamReader reader = new(response.Content);
        Assert.Equal(Script, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetGoogleCloudRunnerDeploymentSetupScriptAsync_SendsTheRequiredProjectIdQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("#!/bin/sh\n", Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PlatformIntegrationsRepository repository = new(connection);

        using GitLabFileResponse response = await repository.GetGoogleCloudRunnerDeploymentSetupScriptAsync(
            5, "my-gcp-project", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/5/google_cloud/setup/runner_deployment_project.sh"
            + "?google_cloud_project_id=my-gcp-project",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(response.Content);
    }

    [Fact]
    public async Task ExchangeTokenAsync_PostsTheAudienceAndExpiry_AndReturnsTheToken()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{ "token": "eyJhbGciOiJSUzI1NiJ9.secret" }""", Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PlatformIntegrationsRepository repository = new(connection);

        TokenExchangeRequest request = new()
        {
            Audience = GitLabTokenExchangeAudience.GitlabArtifactRegistry, ExpiresIn = 600
        };

        GitLabTokenExchangeResult result =
            await repository.ExchangeTokenAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/token_exchange", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"audience\":\"gitlab-artifact-registry\"", sentBody);
        Assert.Contains("\"expires_in\":600", sentBody);
        Assert.Equal("eyJhbGciOiJSUzI1NiJ9.secret", result.Token);

        // The credential must never appear in the record's own rendering.
        Assert.DoesNotContain("eyJhbGciOiJSUzI1NiJ9.secret", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExchangeTokenAsync_OnValidationError_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": "audience is invalid" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        PlatformIntegrationsRepository repository = new(connection);

        TokenExchangeRequest request = new() { Audience = GitLabTokenExchangeAudience.GitlabArtifactRegistry };

        await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.ExchangeTokenAsync(request, TestContext.Current.CancellationToken));
    }
}