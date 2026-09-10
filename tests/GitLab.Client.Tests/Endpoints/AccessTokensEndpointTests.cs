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

public sealed class AccessTokensEndpointTests
{
    private const string TokenJson = """
                                     {
                                       "id": 42,
                                       "name": "deploy-bot",
                                       "revoked": false,
                                       "created_at": "2024-03-01T10:15:30.000Z",
                                       "description": "Token used by the deploy pipeline",
                                       "scopes": ["api", "read_repository"],
                                       "user_id": 1877,
                                       "last_used_at": "2024-05-14T08:00:00.000Z",
                                       "active": true,
                                       "granular": false,
                                       "expires_at": "2025-03-01",
                                       "last_used_ips": ["10.0.0.1", "10.0.0.2"],
                                       "granular_scopes": [
                                         {
                                           "access": "personal_projects",
                                           "permissions": ["read_job"],
                                           "project_id": 3,
                                           "group_id": 5
                                         }
                                       ],
                                       "access_level": 40,
                                       "resource_type": "project",
                                       "resource_id": 7
                                     }
                                     """;

    [Fact]
    public async Task ListForProjectAsync_BuildsProjectRoute_WithQueryOptions_AndDeserializesTokens()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{TokenJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessTokensClient repository = new(connection);

        AccessTokenListOptions options = new()
        {
            Revoked = false,
            State = "active",
            Search = "deploy bot",
            Sort = "expires_at_asc",
            Page = 2,
            PerPage = 20
        };

        List<GitLabAccessToken> tokens = new();
        await foreach (GitLabAccessToken item in
                       repository.ListForProjectAsync(1, options, TestContext.Current.CancellationToken))
        {
            tokens.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/access_tokens", requestUri, StringComparison.Ordinal);
        Assert.Contains("revoked=false", requestUri, StringComparison.Ordinal);
        Assert.Contains("state=active", requestUri, StringComparison.Ordinal);
        Assert.Contains("search=deploy%20bot", requestUri, StringComparison.Ordinal);
        Assert.Contains("sort=expires_at_asc", requestUri, StringComparison.Ordinal);
        Assert.Contains("page=2", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=20", requestUri, StringComparison.Ordinal);

        GitLabAccessToken token = Assert.Single(tokens);
        Assert.Equal(42, token.Id);
        Assert.Equal("deploy-bot", token.Name);
        Assert.False(token.Revoked);
        Assert.Equal("Token used by the deploy pipeline", token.Description);
        Assert.Collection(token.Scopes!,
            scope => Assert.Equal("api", scope),
            scope => Assert.Equal("read_repository", scope));
        Assert.Equal(1877, token.UserId);
        Assert.True(token.Active);
        Assert.False(token.Granular);
        Assert.Equal(new DateTimeOffset(2024, 3, 1, 10, 15, 30, TimeSpan.Zero), token.CreatedAt);
        Assert.Equal(new DateOnly(2025, 3, 1), token.ExpiresAt);
        Assert.Collection(token.LastUsedIps!,
            ip => Assert.Equal("10.0.0.1", ip),
            ip => Assert.Equal("10.0.0.2", ip));
        Assert.Equal(40, token.AccessLevel);
        Assert.Equal("project", token.ResourceType);
        Assert.Equal(7, token.ResourceId);

        // No assertion is possible - or needed - for the plaintext token here: GitLabAccessToken has
        // no such member, so a list or get provably cannot hand one back. Only create and rotate return
        // GitLabAccessTokenWithSecret.

        GitLabGranularScope granularScope = Assert.Single(token.GranularScopes!);
        Assert.Equal("personal_projects", granularScope.Access);
        Assert.Equal("read_job", Assert.Single(granularScope.Permissions!));
        Assert.Equal(3, granularScope.ProjectId);
        Assert.Equal(5, granularScope.GroupId);
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
        AccessTokensClient repository = new(connection);

        await foreach (GitLabAccessToken _ in
                       repository.ListForProjectAsync("gitlab-org/gitlab", null, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/access_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForProjectAsync_BuildsTokenRoute_AndDeserializesToken()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(TokenJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessTokensClient repository = new(connection);

        GitLabAccessToken token =
            await repository.GetForProjectAsync("gitlab-org/gitlab", 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/access_tokens/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(42, token.Id);
        Assert.Equal("deploy-bot", token.Name);
        Assert.Equal(new DateTimeOffset(2024, 5, 14, 8, 0, 0, TimeSpan.Zero), token.LastUsedAt);
    }

    [Fact]
    public async Task CreateForProjectAsync_PostsRequestBody_AndSurfacesThePlaintextToken()
    {
        const string Json = """
                            {
                              "id": 99,
                              "name": "ci-bot",
                              "revoked": false,
                              "scopes": ["api"],
                              "access_level": 30,
                              "expires_at": "2026-01-31",
                              "token": "glpat-XXXXXXXXXXXXXXXXXXXX"
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
        AccessTokensClient repository = new(connection);

        CreateAccessTokenRequest request = new()
        {
            Name = "ci-bot",
            Scopes = ["api"],
            Description = "Pipeline token",
            ExpiresAt = new DateOnly(2026, 1, 31),
            AccessLevel = 30
        };

        GitLabAccessTokenWithSecret token = await repository.CreateForProjectAsync(7, request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/access_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"name\":\"ci-bot\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"scopes\":[\"api\"]", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"description\":\"Pipeline token\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"expires_at\":\"2026-01-31\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"access_level\":30", sentBody, StringComparison.Ordinal);

        Assert.Equal(99, token.Id);
        Assert.Equal("ci-bot", token.Name);
        Assert.Equal(new DateOnly(2026, 1, 31), token.ExpiresAt);
        Assert.Equal("glpat-XXXXXXXXXXXXXXXXXXXX", token.Token);
    }

    [Fact]
    public async Task RotateForProjectAsync_PostsExpiresAt_ToTheRotateRoute()
    {
        const string Json = """
                            {
                              "id": 42,
                              "name": "deploy-bot",
                              "revoked": false,
                              "expires_at": "2026-06-30",
                              "token": "glpat-YYYYYYYYYYYYYYYYYYYY"
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
        AccessTokensClient repository = new(connection);

        GitLabAccessTokenWithSecret token = await repository.RotateForProjectAsync(
            "gitlab-org/gitlab",
            42,
            new RotateAccessTokenRequest { ExpiresAt = new DateOnly(2026, 6, 30) },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/access_tokens/42/rotate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"expires_at\":\"2026-06-30\"}", sentBody);
        Assert.Equal("glpat-YYYYYYYYYYYYYYYYYYYY", token.Token);
        Assert.Equal(new DateOnly(2026, 6, 30), token.ExpiresAt);
    }

    [Fact]
    public async Task RotateForProjectAsync_WithNoRequest_SendsAnEmptyJsonObject()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{ "id": 42, "name": "deploy-bot" }""", Encoding.UTF8,
                    "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessTokensClient repository = new(connection);

        await repository.RotateForProjectAsync(1, 42, cancellationToken: TestContext.Current.CancellationToken);

        // No expiry means "let GitLab choose", not "expires_at: null" - DefaultIgnoreCondition drops it.
        Assert.Equal("{}", sentBody);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/access_tokens/42/rotate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RevokeForProjectAsync_SendsDeleteToTheTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessTokensClient repository = new(connection);

        await repository.RevokeForProjectAsync("gitlab-org/gitlab", 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/access_tokens/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupRoute_AndEncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{TokenJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessTokensClient repository = new(connection);

        List<GitLabAccessToken> tokens = new();
        await foreach (GitLabAccessToken item in repository.ListForGroupAsync(
                           "gitlab-org/subgroup",
                           new AccessTokenListOptions { State = "inactive" },
                           TestContext.Current.CancellationToken))
        {
            tokens.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/groups/gitlab-org%2Fsubgroup/access_tokens", requestUri, StringComparison.Ordinal);
        Assert.Contains("state=inactive", requestUri, StringComparison.Ordinal);
        Assert.Equal("deploy-bot", Assert.Single(tokens).Name);
    }

    [Fact]
    public async Task GetForGroupAsync_BuildsGroupTokenRoute_AndDeserializesToken()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(TokenJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessTokensClient repository = new(connection);

        GitLabAccessToken token = await repository.GetForGroupAsync(9, 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/access_tokens/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("deploy-bot", token.Name);
        Assert.Equal(40, token.AccessLevel);
    }

    [Fact]
    public async Task CreateForGroupAsync_PostsRequestBody_ToTheGroupRoute()
    {
        const string Json = """
                            {
                              "id": 100,
                              "name": "group-bot",
                              "scopes": ["read_api"],
                              "resource_type": "group",
                              "resource_id": 9,
                              "token": "glpat-ZZZZZZZZZZZZZZZZZZZZ"
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
        AccessTokensClient repository = new(connection);

        CreateAccessTokenRequest request = new() { Name = "group-bot", Scopes = ["read_api"] };

        GitLabAccessTokenWithSecret token =
            await repository.CreateForGroupAsync("gitlab-org/subgroup", request,
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/access_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"name\":\"group-bot\",\"scopes\":[\"read_api\"]}", sentBody);
        Assert.Equal("group", token.ResourceType);
        Assert.Equal("glpat-ZZZZZZZZZZZZZZZZZZZZ", token.Token);
    }

    [Fact]
    public async Task RotateForGroupAsync_PostsToTheGroupRotateRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{ "id": 42, "name": "group-bot", "token": "glpat-NEW" }""",
                Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessTokensClient repository = new(connection);

        GitLabAccessTokenWithSecret token = await repository.RotateForGroupAsync(9, 42,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/access_tokens/42/rotate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("glpat-NEW", token.Token);
    }

    [Fact]
    public async Task RevokeForGroupAsync_SendsDeleteToTheGroupTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessTokensClient repository = new(connection);

        await repository.RevokeForGroupAsync(9, 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/access_tokens/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForProjectAsync_OnNotFound_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Access Token Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessTokensClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForProjectAsync(1, 4242, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Access Token Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateForProjectAsync_OnForbidden_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "400 Bad request - Scopes can't be blank" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessTokensClient repository = new(connection);

        CreateAccessTokenRequest request = new() { Name = "ci-bot", Scopes = ["api"] };

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.CreateForProjectAsync(1, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public async Task ListForProjectAsync_ProjectsEveryDateFilterOntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessTokensClient repository = new(connection);

        AccessTokenListOptions options = new()
        {
            CreatedAfter = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBefore = new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero),
            LastUsedAfter = new DateTimeOffset(2025, 2, 3, 4, 5, 6, TimeSpan.Zero),
            LastUsedBefore = new DateTimeOffset(2025, 3, 4, 5, 6, 7, TimeSpan.Zero),
            ExpiresAfter = new DateOnly(2026, 1, 1),
            ExpiresBefore = new DateOnly(2026, 6, 30)
        };

        await foreach (GitLabAccessToken _ in
                       repository.ListForProjectAsync(1, options, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;

        // GitLab types created_* and last_used_* as date-times but expires_* as plain dates; the two must
        // not be rendered the same way or the filter is silently rejected.
        Assert.Contains("created_after=2024-01-01T00:00:00Z", requestUri, StringComparison.Ordinal);
        Assert.Contains("created_before=2024-12-31T23:59:59Z", requestUri, StringComparison.Ordinal);
        Assert.Contains("last_used_after=2025-02-03T04:05:06Z", requestUri, StringComparison.Ordinal);
        Assert.Contains("last_used_before=2025-03-04T05:06:07Z", requestUri, StringComparison.Ordinal);
        Assert.Contains("expires_after=2026-01-01", requestUri, StringComparison.Ordinal);
        Assert.Contains("expires_before=2026-06-30", requestUri, StringComparison.Ordinal);
        Assert.DoesNotContain("expires_after=2026-01-01T", requestUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RotateSelfForProjectAsync_PostsToTheSelfRotateRoute_WithoutATokenId()
    {
        const string Json = """
                            {
                              "id": 42,
                              "name": "deploy-bot",
                              "resource_type": "project",
                              "resource_id": 7,
                              "expires_at": "2026-09-30",
                              "token": "glpat-SELFPROJECT"
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
        AccessTokensClient repository = new(connection);

        GitLabAccessTokenWithSecret token = await repository.RotateSelfForProjectAsync(
            "gitlab-org/gitlab",
            new RotateAccessTokenRequest { ExpiresAt = new DateOnly(2026, 9, 30) },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);

        // "self" is a fixed path word from the route template, so it must appear verbatim - not
        // percent-encoded as if it were a caller-supplied token identifier.
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/access_tokens/self/rotate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"expires_at\":\"2026-09-30\"}", sentBody);
        Assert.Equal("glpat-SELFPROJECT", token.Token);
        Assert.Equal(new DateOnly(2026, 9, 30), token.ExpiresAt);
    }

    [Fact]
    public async Task RotateSelfForGroupAsync_PostsToTheGroupSelfRotateRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{ "id": 42, "name": "group-bot", "token": "glpat-SELFGROUP" }""",
                    Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessTokensClient repository = new(connection);

        GitLabAccessTokenWithSecret token = await repository.RotateSelfForGroupAsync(
            "gitlab-org/subgroup",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/access_tokens/self/rotate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{}", sentBody);
        Assert.Equal("glpat-SELFGROUP", token.Token);
    }

    [Fact]
    public void GitLabAccessTokenWithSecret_ToString_DoesNotPrintTheSecret()
    {
        GitLabAccessTokenWithSecret token = new()
        {
            Id = 42, Name = "deploy-bot", Token = "glpat-DO-NOT-PRINT-ME", AccessLevel = 40
        };

        string rendered = token.ToString();

        // The compiler-generated record ToString() would print every member, which is exactly how a
        // credential reaches a log file.
        Assert.DoesNotContain("glpat-DO-NOT-PRINT-ME", rendered, StringComparison.Ordinal);
        Assert.Contains("redacted", rendered, StringComparison.Ordinal);
        Assert.Contains("deploy-bot", rendered, StringComparison.Ordinal);
    }
}