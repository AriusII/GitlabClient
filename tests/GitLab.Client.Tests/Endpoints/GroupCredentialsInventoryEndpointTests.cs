using System.Net;
using System.Text;

using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class GroupCredentialsInventoryEndpointTests
{
    private const string PersonalAccessTokenJson = """
                                                   {
                                                     "id": 2,
                                                     "name": "release-automation",
                                                     "revoked": false,
                                                     "created_at": "2024-02-02T09:00:00.000Z",
                                                     "scopes": ["api"],
                                                     "user_id": 3,
                                                     "active": true,
                                                     "expires_at": "2025-08-31T15:53:00.073Z"
                                                   }
                                                   """;

    private const string AccessTokenJson = """
                                           {
                                             "id": 9,
                                             "name": "deploy-bot",
                                             "revoked": false,
                                             "user_id": 11,
                                             "active": true,
                                             "access_level": 40,
                                             "resource_type": "project",
                                             "resource_id": 6
                                           }
                                           """;

    private const string SshKeyJson = """
                                      {
                                        "id": 7,
                                        "title": "laptop-key",
                                        "created_at": "2024-01-15T10:00:00.000Z",
                                        "expires_at": "2026-01-15T00:00:00.000Z",
                                        "last_used_at": "2024-09-01T08:00:00.000Z",
                                        "usage_type": "auth_and_signing",
                                        "user_id": 3
                                      }
                                      """;

    [Fact]
    public async Task ListPersonalAccessTokensAsync_BuildsManageRoute_WithQueryOptions_AndEncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{PersonalAccessTokenJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupCredentialsInventoryClient repository = new(connection);

        PersonalAccessTokenListOptions options = new() { Revoked = false, PerPage = 25 };

        List<GitLabPersonalAccessToken> tokens = new();
        await foreach (GitLabPersonalAccessToken item in repository.ListPersonalAccessTokensAsync(
                           "parent-group/subgroup", options, TestContext.Current.CancellationToken))
        {
            tokens.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.StartsWith(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/manage/personal_access_tokens",
            requestUri, StringComparison.Ordinal);
        Assert.Contains("revoked=false", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=25", requestUri, StringComparison.Ordinal);

        GitLabPersonalAccessToken token = Assert.Single(tokens);
        Assert.Equal(2, token.Id);
        Assert.Equal("release-automation", token.Name);
    }

    [Fact]
    public async Task RevokePersonalAccessTokenAsync_SendsDeleteToTheTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupCredentialsInventoryClient repository = new(connection);

        await repository.RevokePersonalAccessTokenAsync(5, 2, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/manage/personal_access_tokens/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RotatePersonalAccessTokenAsync_PostsToTheRotateRoute_AndSurfacesThePlaintextToken()
    {
        const string Json = """
                            {
                              "id": 2,
                              "name": "release-automation",
                              "revoked": false,
                              "expires_at": "2026-04-30T00:00:00.000Z",
                              "token": "glpat-ROTATED"
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

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupCredentialsInventoryClient repository = new(connection);

        GitLabPersonalAccessTokenWithSecret token = await repository.RotatePersonalAccessTokenAsync(
            5, 2, new RotateAccessTokenRequest { ExpiresAt = new DateOnly(2026, 4, 30) },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/manage/personal_access_tokens/2/rotate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"expires_at\":\"2026-04-30\"}", sentBody);
        Assert.Equal("glpat-ROTATED", token.Token);
    }

    [Fact]
    public async Task ListResourceAccessTokensAsync_BuildsManageRoute_WithQueryOptions()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{AccessTokenJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupCredentialsInventoryClient repository = new(connection);

        AccessTokenListOptions options = new() { State = "active" };

        List<GitLabAccessToken> tokens = new();
        await foreach (GitLabAccessToken item in repository.ListResourceAccessTokensAsync(
                           5, options, TestContext.Current.CancellationToken))
        {
            tokens.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.StartsWith("https://gitlab.example/api/v4/groups/5/manage/resource_access_tokens",
            requestUri, StringComparison.Ordinal);
        Assert.Contains("state=active", requestUri, StringComparison.Ordinal);

        GitLabAccessToken token = Assert.Single(tokens);
        Assert.Equal(9, token.Id);
        Assert.Equal("project", token.ResourceType);
        Assert.Equal(6, token.ResourceId);
    }

    [Fact]
    public async Task RevokeResourceAccessTokenAsync_SendsDelete_WithOptionalExpiresAtQuery()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupCredentialsInventoryClient repository = new(connection);

        await repository.RevokeResourceAccessTokenAsync(5, 9, new DateOnly(2026, 1, 1),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/manage/resource_access_tokens/9?expires_at=2026-01-01",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RevokeResourceAccessTokenAsync_WithoutExpiresAt_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupCredentialsInventoryClient repository = new(connection);

        await repository.RevokeResourceAccessTokenAsync(5, 9,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/5/manage/resource_access_tokens/9",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RotateResourceAccessTokenAsync_PostsExpiresAt_AndReturnsNoContent()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupCredentialsInventoryClient repository = new(connection);

        await repository.RotateResourceAccessTokenAsync(5, 9,
            new RotateAccessTokenRequest { ExpiresAt = new DateOnly(2026, 6, 1) },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/manage/resource_access_tokens/9/rotate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"expires_at\":\"2026-06-01\"}", sentBody);
    }

    [Fact]
    public async Task ListSshKeysAsync_BuildsManageRoute_WithQueryOptions_AndDeserializesKeys()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{SshKeyJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupCredentialsInventoryClient repository = new(connection);

        GroupManagedSshKeyListOptions options = new()
        {
            CreatedAfter = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        List<GitLabGroupManagedSshKey> keys = new();
        await foreach (GitLabGroupManagedSshKey item in repository.ListSshKeysAsync(
                           5, options, TestContext.Current.CancellationToken))
        {
            keys.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.StartsWith("https://gitlab.example/api/v4/groups/5/manage/ssh_keys",
            requestUri, StringComparison.Ordinal);
        Assert.Contains("created_after=", requestUri, StringComparison.Ordinal);

        GitLabGroupManagedSshKey key = Assert.Single(keys);
        Assert.Equal(7, key.Id);
        Assert.Equal("laptop-key", key.Title);
        Assert.Equal("auth_and_signing", key.UsageType);
        Assert.Equal(3, key.UserId);
    }

    [Fact]
    public async Task DeleteSshKeyAsync_SendsDeleteToTheKeyRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        GroupCredentialsInventoryClient repository = new(connection);

        await repository.DeleteSshKeyAsync(5, 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/manage/ssh_keys/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}