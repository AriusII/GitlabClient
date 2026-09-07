using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ApplicationsRepositoryTests
{
    private const string ApplicationJson = """
                                           {
                                             "id": 5,
                                             "application_id": "2a3e5b5b0a04d2d1b8a4b3e7f3e4e9c1",
                                             "application_name": "MyApp",
                                             "callback_url": "https://example.com/oauth/callback",
                                             "confidential": true,
                                             "scopes": ["api", "read_user"]
                                           }
                                           """;

    private const string ApplicationWithSecretJson = """
                                                     {
                                                       "id": 5,
                                                       "application_id": "2a3e5b5b0a04d2d1b8a4b3e7f3e4e9c1",
                                                       "application_name": "MyApp",
                                                       "callback_url": "https://example.com/oauth/callback",
                                                       "confidential": true,
                                                       "scopes": ["api", "read_user"],
                                                       "secret": "topsecretvalue"
                                                     }
                                                     """;

    [Fact]
    public async Task ListAsync_BuildsApplicationsRoute_AndDeserializesWithoutSecret()
    {
        string json = $"[{ApplicationJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ApplicationsRepository repository = new(connection);

        List<GitLabApplication> applications = new();
        await foreach (GitLabApplication item in repository.ListAsync(TestContext.Current.CancellationToken))
        {
            applications.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/applications", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabApplication application = Assert.Single(applications);
        Assert.Equal(5, application.Id);
        Assert.Equal("MyApp", application.ApplicationName);
        Assert.True(application.Confidential);
        Assert.Equal(["api", "read_user"], application.Scopes);
    }

    [Fact]
    public async Task CreateAsync_PostsTheApplicationBody_AndReturnsTheSecret()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ApplicationWithSecretJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ApplicationsRepository repository = new(connection);

        CreateApplicationRequest request = new()
        {
            Name = "MyApp", RedirectUri = "https://example.com/oauth/callback", Scopes = "api read_user"
        };

        GitLabApplicationWithSecret application =
            await repository.CreateAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/applications", handler.LastRequest?.RequestUri?.AbsoluteUri);

        // Confidential is unset and must be omitted, not sent as a JSON null.
        Assert.Equal(
            """{"name":"MyApp","redirect_uri":"https://example.com/oauth/callback","scopes":"api read_user"}""",
            sentBody);

        Assert.Equal("topsecretvalue", application.Secret);
        Assert.DoesNotContain("topsecretvalue", application.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheApplicationIdRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ApplicationsRepository repository = new(connection);

        await repository.DeleteAsync(5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/applications/5", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RenewSecretAsync_PostsToTheRenewSecretRoute_WithNoBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ApplicationWithSecretJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ApplicationsRepository repository = new(connection);

        GitLabApplicationWithSecret application =
            await repository.RenewSecretAsync(5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/applications/5/renew-secret",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal("topsecretvalue", application.Secret);
    }

    [Fact]
    public async Task ListForCurrentUserAsync_BuildsUserApplicationsRoute()
    {
        string json = $"[{ApplicationJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ApplicationsRepository repository = new(connection);

        List<GitLabApplication> applications = new();
        await foreach (GitLabApplication item in
                       repository.ListForCurrentUserAsync(TestContext.Current.CancellationToken))
        {
            applications.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/user/applications",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("MyApp", Assert.Single(applications).ApplicationName);
    }

    [Fact]
    public async Task CreateForCurrentUserAsync_PostsToTheUserApplicationsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(ApplicationWithSecretJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ApplicationsRepository repository = new(connection);

        CreateApplicationRequest request = new()
        {
            Name = "MyApp", RedirectUri = "https://example.com/oauth/callback", Scopes = "api", Confidential = false
        };

        GitLabApplicationWithSecret application =
            await repository.CreateForCurrentUserAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/applications",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(5, application.Id);
    }

    [Fact]
    public async Task GetForCurrentUserAsync_BuildsTheApplicationIdRoute_AndDeserializesWithoutSecret()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ApplicationJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ApplicationsRepository repository = new(connection);

        GitLabApplication application = await repository.GetForCurrentUserAsync(5,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/applications/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("MyApp", application.ApplicationName);
    }

    [Fact]
    public async Task UpdateForCurrentUserAsync_PutsOnlyTheChangedFields()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ApplicationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ApplicationsRepository repository = new(connection);

        UpdateApplicationRequest request = new() { Scopes = "api read_user" };

        GitLabApplication application =
            await repository.UpdateForCurrentUserAsync(5, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/applications/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"scopes":"api read_user"}""", sentBody);
        Assert.Equal(5, application.Id);
    }

    [Fact]
    public async Task DeleteForCurrentUserAsync_SendsDeleteToTheApplicationIdRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ApplicationsRepository repository = new(connection);

        await repository.DeleteForCurrentUserAsync(5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/user/applications/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForCurrentUserAsync_OnMissingApplication_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ApplicationsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForCurrentUserAsync(999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}