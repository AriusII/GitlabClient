using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class DeployTokensEndpointTests
{
    private const string DeployTokenJson = """
                                           {
                                             "id": 1,
                                             "name": "MyToken",
                                             "username": "gitlab+deploy-token-1",
                                             "expires_at": "2020-02-14T00:00:00.000Z",
                                             "scopes": [
                                               "read_repository",
                                               "read_registry"
                                             ],
                                             "revoked": false,
                                             "expired": false
                                           }
                                           """;

    private const string DeployTokenWithSecretJson = """
                                                     {
                                                       "id": 1,
                                                       "name": "My deploy token",
                                                       "username": "custom-user",
                                                       "expires_at": "2021-01-01T00:00:00.000Z",
                                                       "token": "jMRvtPNxrn3crTAGukpZ",
                                                       "scopes": [
                                                         "read_repository"
                                                       ],
                                                       "revoked": false,
                                                       "expired": false
                                                     }
                                                     """;

    [Fact]
    public async Task ListAsync_BuildsInstanceRoute_AndDeserializesTheToken()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{DeployTokenJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DeployTokensClient repository = new(connection);

        List<GitLabDeployToken> tokens = [];
        await foreach (GitLabDeployToken item in repository.ListAsync(
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            tokens.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/deploy_tokens", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabDeployToken token = Assert.Single(tokens);
        Assert.Equal(1, token.Id);
        Assert.Equal("MyToken", token.Name);
        Assert.Equal("gitlab+deploy-token-1", token.Username);
        Assert.Equal(new DateTimeOffset(2020, 2, 14, 0, 0, 0, TimeSpan.Zero), token.ExpiresAt);
        Assert.NotNull(token.Scopes);
        Assert.Equal(2, token.Scopes!.Count);
        Assert.Equal("read_repository", token.Scopes[0]);
        Assert.Equal("read_registry", token.Scopes[1]);
        Assert.False(token.Revoked);
        Assert.False(token.Expired);
    }

    [Fact]
    public async Task ListAsync_ProjectsTheListOptionsOntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DeployTokensClient repository = new(connection);

        DeployTokenListOptions options = new() { Active = true, Page = 2, PerPage = 50 };

        await foreach (GitLabDeployToken unused in repository.ListAsync(options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/deploy_tokens?active=true&page=2&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DeployTokensClient repository = new(connection);

        await foreach (GitLabDeployToken unused in repository.ListForProjectAsync("gitlab-org/gitlab",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/deploy_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupRoute_WithTheActiveFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{DeployTokenJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DeployTokensClient repository = new(connection);

        List<GitLabDeployToken> tokens = [];
        await foreach (GitLabDeployToken item in repository.ListForGroupAsync("parent-group/subgroup",
                           new DeployTokenListOptions { Active = false }, TestContext.Current.CancellationToken))
        {
            tokens.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/deploy_tokens?active=false",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("MyToken", Assert.Single(tokens).Name);
    }

    [Fact]
    public async Task GetForProjectAsync_BuildsTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(DeployTokenJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DeployTokensClient repository = new(connection);

        GitLabDeployToken token = await repository.GetForProjectAsync(42, 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/deploy_tokens/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, token.Id);
    }

    [Fact]
    public async Task GetForGroupAsync_BuildsTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(DeployTokenJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DeployTokensClient repository = new(connection);

        GitLabDeployToken token = await repository.GetForGroupAsync(9970, 7, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/deploy_tokens/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("gitlab+deploy-token-1", token.Username);
    }

    [Fact]
    public async Task CreateForProjectAsync_PostsTheRequest_AndReturnsTheOneTimeSecret()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(DeployTokenWithSecretJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DeployTokensClient repository = new(connection);

        CreateDeployTokenRequest request = new()
        {
            Name = "My deploy token",
            Scopes = [GitLabDeployTokenScopes.ReadRepository],
            ExpiresAt = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Username = "custom-user"
        };

        GitLabDeployTokenWithSecret created =
            await repository.CreateForProjectAsync(5, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/deploy_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """
            {"name":"My deploy token","scopes":["read_repository"],"expires_at":"2021-01-01T00:00:00+00:00","username":"custom-user"}
            """,
            sentBody);

        Assert.Equal("jMRvtPNxrn3crTAGukpZ", created.Token);
        Assert.Equal("custom-user", created.Username);
    }

    [Fact]
    public async Task CreateForGroupAsync_OmitsTheOptionalMembersItWasNotGiven()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(DeployTokenWithSecretJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DeployTokensClient repository = new(connection);

        CreateDeployTokenRequest request = new()
        {
            Name = "group token",
            Scopes = [GitLabDeployTokenScopes.ReadPackageRegistry, GitLabDeployTokenScopes.WritePackageRegistry]
        };

        GitLabDeployTokenWithSecret created = await repository.CreateForGroupAsync("parent-group/subgroup", request,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/deploy_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """
            {"name":"group token","scopes":["read_package_registry","write_package_registry"]}
            """,
            sentBody);
        Assert.Equal(1, created.Id);
    }

    [Fact]
    public async Task DeleteForProjectAsync_SendsDeleteToTheTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DeployTokensClient repository = new(connection);

        await repository.DeleteForProjectAsync("gitlab-org/gitlab", 13, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/deploy_tokens/13",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteForGroupAsync_SendsDeleteToTheTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DeployTokensClient repository = new(connection);

        await repository.DeleteForGroupAsync(9970, 13, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/deploy_tokens/13",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListAsync_SurfacesForbiddenAsTheTypedException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{"message":"403 Forbidden"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DeployTokensClient repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(async () =>
        {
            await foreach (GitLabDeployToken unused in repository
                               .ListAsync(cancellationToken: TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                // The exception is thrown while fetching the first page, before any item is yielded.
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public void GitLabDeployTokenWithSecret_ToString_RedactsTheSecret()
    {
        GitLabDeployTokenWithSecret token = new()
        {
            Id = 1, Name = "MyToken", Username = "gitlab+deploy-token-1", Token = "jMRvtPNxrn3crTAGukpZ"
        };

        string rendered = token.ToString();

        Assert.DoesNotContain("jMRvtPNxrn3crTAGukpZ", rendered, StringComparison.Ordinal);
        Assert.Contains("<redacted>", rendered, StringComparison.Ordinal);
    }

    [Fact]
    public void GitLabDeployToken_HasNoTokenMember()
    {
        // The read-side DTO must be structurally incapable of carrying a secret - that is the whole
        // reason GitLabDeployTokenWithSecret exists as a second type.
        Assert.Null(typeof(GitLabDeployToken).GetProperty("Token"));
    }
}