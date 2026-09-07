using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PersonalAccessTokensRepositoryTests
{
    private const string TokenJson = """
                                     {
                                       "id": 2,
                                       "name": "release-automation",
                                       "revoked": false,
                                       "created_at": "2024-02-02T09:00:00.000Z",
                                       "description": "Token to manage api",
                                       "scopes": ["api", "read_user"],
                                       "user_id": 3,
                                       "last_used_at": "2024-08-31T15:53:00.073Z",
                                       "active": true,
                                       "granular": true,
                                       "expires_at": "2025-08-31T15:53:00.073Z",
                                       "last_used_ips": ["203.0.113.7"],
                                       "granular_scopes": [
                                         {
                                           "access": "personal_projects",
                                           "permissions": ["read_job"],
                                           "project_id": 3,
                                           "group_id": 5
                                         }
                                       ]
                                     }
                                     """;

    [Fact]
    public async Task ListAsync_BuildsPersonalAccessTokensRoute_WithQueryOptions_AndDeserializesTokens()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{TokenJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        PersonalAccessTokenListOptions options = new()
        {
            UserId = 3,
            Revoked = false,
            State = "active",
            Search = "release automation",
            Sort = "created_at_desc",
            PerPage = 50
        };

        List<GitLabPersonalAccessToken> tokens = new();
        await foreach (GitLabPersonalAccessToken item in
                       repository.ListAsync(options, TestContext.Current.CancellationToken))
        {
            tokens.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/personal_access_tokens?", requestUri, StringComparison.Ordinal);
        Assert.Contains("user_id=3", requestUri, StringComparison.Ordinal);
        Assert.Contains("revoked=false", requestUri, StringComparison.Ordinal);
        Assert.Contains("state=active", requestUri, StringComparison.Ordinal);
        Assert.Contains("search=release%20automation", requestUri, StringComparison.Ordinal);
        Assert.Contains("sort=created_at_desc", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=50", requestUri, StringComparison.Ordinal);

        GitLabPersonalAccessToken token = Assert.Single(tokens);
        Assert.Equal(2, token.Id);
        Assert.Equal("release-automation", token.Name);
        Assert.False(token.Revoked);
        Assert.Equal("Token to manage api", token.Description);
        Assert.Collection(token.Scopes!,
            scope => Assert.Equal("api", scope),
            scope => Assert.Equal("read_user", scope));
        Assert.Equal(3, token.UserId);
        Assert.True(token.Active);
        Assert.True(token.Granular);
        Assert.Equal(new DateTimeOffset(2024, 2, 2, 9, 0, 0, TimeSpan.Zero), token.CreatedAt);
        Assert.Equal("203.0.113.7", Assert.Single(token.LastUsedIps!));

        // No assertion is possible - or needed - for the plaintext token here: GitLabPersonalAccessToken
        // has no such member, so a listing provably cannot hand one back. Only create and rotate return
        // GitLabPersonalAccessTokenWithSecret.

        GitLabGranularScope granularScope = Assert.Single(token.GranularScopes!);
        Assert.Equal("personal_projects", granularScope.Access);
        Assert.Equal("read_job", Assert.Single(granularScope.Permissions!));
        Assert.Equal(3, granularScope.ProjectId);
        Assert.Equal(5, granularScope.GroupId);
    }

    [Fact]
    public async Task ListAsync_WithoutOptions_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        await foreach (GitLabPersonalAccessToken _ in
                       repository.ListAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/personal_access_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_BuildsTokenRoute_AndDeserializesToken()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(TokenJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        GitLabPersonalAccessToken token = await repository.GetAsync(2, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/personal_access_tokens/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, token.Id);
        Assert.Equal("release-automation", token.Name);
        Assert.Equal(new DateTimeOffset(2025, 8, 31, 15, 53, 0, 73, TimeSpan.Zero), token.ExpiresAt);
    }

    [Fact]
    public async Task RevokeAsync_SendsDeleteToTheTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        await repository.RevokeAsync(2, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/personal_access_tokens/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RotateAsync_PostsExpiresAt_ToTheRotateRoute_AndSurfacesThePlaintextToken()
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

            // GitLab answers 201 here but 200 on /self/rotate; both are success and neither is branched on.
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        GitLabPersonalAccessTokenWithSecret token = await repository.RotateAsync(
            2,
            new RotateAccessTokenRequest { ExpiresAt = new DateOnly(2026, 4, 30) },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/personal_access_tokens/2/rotate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal("{\"expires_at\":\"2026-04-30\"}", sentBody);
        Assert.Equal("glpat-ROTATED", token.Token);
        Assert.Equal(new DateTimeOffset(2026, 4, 30, 0, 0, 0, TimeSpan.Zero), token.ExpiresAt);
    }

    [Fact]
    public async Task GetSelfAsync_BuildsTheSelfRoute_AsALiteralSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(TokenJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        GitLabPersonalAccessToken token = await repository.GetSelfAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/personal_access_tokens/self",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, token.Id);
        Assert.Equal(3, token.UserId);
    }

    [Fact]
    public async Task RevokeSelfAsync_SendsDeleteToTheSelfRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        await repository.RevokeSelfAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/personal_access_tokens/self",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RotateSelfAsync_WithNoRequest_PostsAnEmptyJsonObjectToTheSelfRotateRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{ "id": 2, "name": "release-automation", "token": "glpat-SELF" }""",
                    Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        GitLabPersonalAccessTokenWithSecret token =
            await repository.RotateSelfAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/personal_access_tokens/self/rotate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{}", sentBody);
        Assert.Equal("glpat-SELF", token.Token);
    }

    [Fact]
    public async Task CreateForUserAsync_PostsRequestBody_ToThePluralUsersRoute()
    {
        const string Json = """
                            {
                              "id": 11,
                              "name": "bot-token",
                              "revoked": false,
                              "scopes": ["api"],
                              "user_id": 88,
                              "expires_at": "2026-12-31T00:00:00.000Z",
                              "token": "glpat-CREATED"
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
        PersonalAccessTokensRepository repository = new(connection);

        CreatePersonalAccessTokenRequest request = new()
        {
            Name = "bot-token", Scopes = ["api"], Description = "Automation", ExpiresAt = new DateOnly(2026, 12, 31)
        };

        GitLabPersonalAccessTokenWithSecret token =
            await repository.CreateForUserAsync(88, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/88/personal_access_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"name\":\"bot-token\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"scopes\":[\"api\"]", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"description\":\"Automation\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"expires_at\":\"2026-12-31\"", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("access_level", sentBody, StringComparison.Ordinal);

        Assert.Equal(11, token.Id);
        Assert.Equal(88, token.UserId);
        Assert.Equal("glpat-CREATED", token.Token);
    }

    [Fact]
    public async Task GetSelfAsync_WhenTheCredentialIsNotAPersonalAccessToken_ThrowsGitLabAuthenticationException()
    {
        const string Json = """{ "message": "401 Unauthorized" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabAuthenticationException>(() =>
            repository.GetSelfAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal("401 Unauthorized", exception.Message);
    }

    [Fact]
    public async Task GetAsync_OnNotFound_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(9999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }

    [Fact]
    public async Task CreateForCurrentUserAsync_PostsToTheSingularUserRoute()
    {
        const string Json = """
                            {
                              "id": 21,
                              "name": "kubectl",
                              "revoked": false,
                              "scopes": ["k8s_proxy"],
                              "user_id": 3,
                              "expires_at": "2026-05-01T00:00:00.000Z",
                              "token": "glpat-CURRENTUSER"
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
        PersonalAccessTokensRepository repository = new(connection);

        CreateCurrentUserPersonalAccessTokenRequest request = new()
        {
            Name = "kubectl",
            Scopes = [GitLabTokenScopes.K8sProxy, GitLabTokenScopes.SelfRotate],
            ExpiresAt = new DateOnly(2026, 5, 1)
        };

        GitLabPersonalAccessTokenWithSecret token =
            await repository.CreateForCurrentUserAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);

        // Singular "user" - the caller's own account. The plural "users/:id" route is the administrator
        // one and confusing the two silently targets the wrong account.
        Assert.Equal("https://gitlab.example/api/v4/user/personal_access_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"name\":\"kubectl\",\"scopes\":[\"k8s_proxy\",\"self_rotate\"],\"expires_at\":\"2026-05-01\"}",
            sentBody);
        Assert.Equal("glpat-CURRENTUSER", token.Token);
        Assert.Equal(21, token.Id);
    }

    [Fact]
    public async Task GetSelfAssociationsAsync_BuildsTheAssociationsRoute_AndDeserializesTheEnvelope()
    {
        const string Json = """
                            {
                              "groups": [
                                {
                                  "id": 1,
                                  "web_url": "https://gitlab.example/groups/test",
                                  "name": "Test",
                                  "parent_id": null,
                                  "organization_id": 1,
                                  "access_level": 50,
                                  "visibility": "public"
                                }
                              ],
                              "projects": [
                                {
                                  "id": 7,
                                  "description": "A test project",
                                  "name": "Gitlab Test",
                                  "name_with_namespace": "Test / Gitlab Test",
                                  "path": "gitlab-test",
                                  "path_with_namespace": "test/gitlab-test",
                                  "created_at": "2024-01-15T09:00:00.000Z",
                                  "access_level": 40,
                                  "visibility": "private",
                                  "web_url": "https://gitlab.example/test/gitlab-test"
                                }
                              ]
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        GitLabTokenAssociations associations = await repository.GetSelfAssociationsAsync(
            new TokenAssociationListOptions { MinAccessLevel = 30, Page = 2, PerPage = 10 },
            TestContext.Current.CancellationToken);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/personal_access_tokens/self/associations?", requestUri, StringComparison.Ordinal);
        Assert.Contains("min_access_level=30", requestUri, StringComparison.Ordinal);
        Assert.Contains("page=2", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=10", requestUri, StringComparison.Ordinal);

        GitLabTokenAssociationGroup group = Assert.Single(associations.Groups!);
        Assert.Equal(1, group.Id);
        Assert.Equal("Test", group.Name);
        Assert.Null(group.ParentId);
        Assert.Equal(1, group.OrganizationId);
        Assert.Equal(50, group.AccessLevel);
        Assert.Equal(new Uri("https://gitlab.example/groups/test"), group.WebUrl);

        GitLabTokenAssociationProject project = Assert.Single(associations.Projects!);
        Assert.Equal(7, project.Id);
        Assert.Equal("Test / Gitlab Test", project.NameWithNamespace);
        Assert.Equal("test/gitlab-test", project.PathWithNamespace);
        Assert.Equal(40, project.AccessLevel);
        Assert.Equal(new DateTimeOffset(2024, 1, 15, 9, 0, 0, TimeSpan.Zero), project.CreatedAt);
    }

    [Fact]
    public async Task ListForProjectServiceAccountAsync_BuildsTheServiceAccountRoute_AndEncodesTheProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{TokenJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        List<GitLabPersonalAccessToken> tokens = new();
        await foreach (GitLabPersonalAccessToken item in repository.ListForProjectServiceAccountAsync(
                           "gitlab-org/gitlab",
                           57,
                           new AccessTokenListOptions { State = "active", PerPage = 5 },
                           TestContext.Current.CancellationToken))
        {
            tokens.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/gitlab-org%2Fgitlab/service_accounts/57/personal_access_tokens",
            requestUri, StringComparison.Ordinal);
        Assert.Contains("state=active", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=5", requestUri, StringComparison.Ordinal);
        Assert.Equal("release-automation", Assert.Single(tokens).Name);
    }

    [Fact]
    public async Task CreateForProjectServiceAccountAsync_PostsToTheServiceAccountRoute()
    {
        const string Json = """
                            {
                              "id": 31,
                              "name": "svc-token",
                              "user_id": 57,
                              "scopes": ["api"],
                              "token": "glpat-SVCPROJECT"
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
        PersonalAccessTokensRepository repository = new(connection);

        CreatePersonalAccessTokenRequest request = new() { Name = "svc-token", Scopes = [GitLabTokenScopes.Api] };

        GitLabPersonalAccessTokenWithSecret token = await repository.CreateForProjectServiceAccountAsync(
            7, 57, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/service_accounts/57/personal_access_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"name\":\"svc-token\",\"scopes\":[\"api\"]}", sentBody);
        Assert.Equal("glpat-SVCPROJECT", token.Token);
    }

    [Fact]
    public async Task RotateForProjectServiceAccountAsync_PostsToTheServiceAccountRotateRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("""{ "id": 31, "name": "svc-token", "token": "glpat-SVCROTATED" }""",
                Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        GitLabPersonalAccessTokenWithSecret token = await repository.RotateForProjectServiceAccountAsync(
            7, 57, 31, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/service_accounts/57/personal_access_tokens/31/rotate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("glpat-SVCROTATED", token.Token);
    }

    [Fact]
    public async Task RevokeForProjectServiceAccountAsync_SendsDeleteToTheServiceAccountTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        await repository.RevokeForProjectServiceAccountAsync(7, 57, 31, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/service_accounts/57/personal_access_tokens/31",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForGroupServiceAccountAsync_BuildsTheGroupServiceAccountRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        await foreach (GitLabPersonalAccessToken _ in repository.ListForGroupServiceAccountAsync(
                           "gitlab-org/subgroup", 57, null, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/service_accounts/57/personal_access_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateForGroupServiceAccountAsync_PostsToTheGroupServiceAccountRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("""{ "id": 32, "name": "svc-token", "token": "glpat-SVCGROUP" }""",
                Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        CreatePersonalAccessTokenRequest request = new() { Name = "svc-token", Scopes = ["read_api"] };

        GitLabPersonalAccessTokenWithSecret token = await repository.CreateForGroupServiceAccountAsync(
            9, 57, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/service_accounts/57/personal_access_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("glpat-SVCGROUP", token.Token);
    }

    [Fact]
    public async Task RotateForGroupServiceAccountAsync_PostsToTheGroupServiceAccountRotateRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{ "id": 32, "name": "svc-token", "token": "glpat-SVCGROUPROT" }""",
                    Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        GitLabPersonalAccessTokenWithSecret token = await repository.RotateForGroupServiceAccountAsync(
            9,
            57,
            32,
            new RotateAccessTokenRequest { ExpiresAt = new DateOnly(2026, 11, 30) },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9/service_accounts/57/personal_access_tokens/32/rotate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"expires_at\":\"2026-11-30\"}", sentBody);
        Assert.Equal("glpat-SVCGROUPROT", token.Token);
    }

    [Fact]
    public async Task RevokeForGroupServiceAccountAsync_SendsDeleteToTheGroupServiceAccountTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        await repository.RevokeForGroupServiceAccountAsync(9, 57, 32, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/service_accounts/57/personal_access_tokens/32",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListImpersonationTokensAsync_BuildsTheRoute_WithStateFilter_AndDeserializes()
    {
        // A real GitLab payload: "impersonation" is the boolean column, not the string the pinned spec
        // declares, and "expires_at" comes back as a plain date on this entity.
        const string Json = """
                            [
                              {
                                "id": 2,
                                "name": "mytoken",
                                "revoked": false,
                                "created_at": "2024-03-17T17:18:09.283Z",
                                "scopes": ["api"],
                                "user_id": 42,
                                "active": true,
                                "granular": false,
                                "impersonation": true,
                                "expires_at": "2025-04-04",
                                "last_used_ips": ["198.51.100.4"]
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        List<GitLabImpersonationToken> tokens = new();
        await foreach (GitLabImpersonationToken item in repository.ListImpersonationTokensAsync(
                           42,
                           new ImpersonationTokenListOptions { State = "active", PerPage = 25 },
                           TestContext.Current.CancellationToken))
        {
            tokens.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/users/42/impersonation_tokens?", requestUri, StringComparison.Ordinal);
        Assert.Contains("state=active", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=25", requestUri, StringComparison.Ordinal);

        GitLabImpersonationToken token = Assert.Single(tokens);
        Assert.Equal(2, token.Id);
        Assert.Equal("mytoken", token.Name);
        Assert.True(token.Impersonation);
        Assert.Equal(42, token.UserId);
        // GitLab returns a plain date for expires_at on this entity. System.Text.Json parses a
        // date-only ISO 8601 string into a DateTimeOffset at the machine local offset, not UTC, so
        // assert the calendar date rather than an instant.
        Assert.Equal(new DateTime(2025, 4, 4, 0, 0, 0, DateTimeKind.Unspecified), token.ExpiresAt?.DateTime);
        Assert.Equal("198.51.100.4", Assert.Single(token.LastUsedIps!));
    }

    [Fact]
    public async Task GetImpersonationTokenAsync_BuildsTheImpersonationTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{ "id": 2, "name": "mytoken", "impersonation": true }""",
                Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        GitLabImpersonationToken token =
            await repository.GetImpersonationTokenAsync(42, 2, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/42/impersonation_tokens/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("mytoken", token.Name);
    }

    [Fact]
    public async Task CreateImpersonationTokenAsync_PostsTheRequestBody_AndSurfacesThePlaintextToken()
    {
        const string Json = """
                            {
                              "id": 5,
                              "name": "impersonate-me",
                              "impersonation": true,
                              "scopes": ["api", "read_user"],
                              "user_id": 42,
                              "expires_at": "2026-02-28",
                              "token": "glpat-IMPERSONATION"
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
        PersonalAccessTokensRepository repository = new(connection);

        CreateImpersonationTokenRequest request = new()
        {
            Name = "impersonate-me",
            Scopes = [GitLabTokenScopes.Api, GitLabTokenScopes.ReadUser],
            Description = "Support session",
            ExpiresAt = new DateOnly(2026, 2, 28)
        };

        GitLabImpersonationTokenWithSecret token =
            await repository.CreateImpersonationTokenAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/42/impersonation_tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"name\":\"impersonate-me\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"scopes\":[\"api\",\"read_user\"]", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"expires_at\":\"2026-02-28\"", sentBody, StringComparison.Ordinal);

        // Nothing was set, so the mutually exclusive granular form must not be sent at all - an empty
        // array would read as "grant no granular permissions" rather than "do not use granular scopes".
        Assert.DoesNotContain("granular_scopes", sentBody, StringComparison.Ordinal);

        Assert.Equal("glpat-IMPERSONATION", token.Token);
        Assert.True(token.Impersonation);
    }

    [Fact]
    public async Task RevokeImpersonationTokenAsync_SendsDeleteToTheImpersonationTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PersonalAccessTokensRepository repository = new(connection);

        await repository.RevokeImpersonationTokenAsync(42, 2, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/users/42/impersonation_tokens/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public void SecretCarryingTokens_ToString_DoNotPrintTheSecret()
    {
        GitLabPersonalAccessTokenWithSecret personal = new()
        {
            Id = 2, Name = "release-automation", Token = "glpat-DO-NOT-PRINT-ME"
        };

        GitLabImpersonationTokenWithSecret impersonation = new()
        {
            Id = 5, Name = "impersonate-me", Token = "glpat-DO-NOT-PRINT-ME-EITHER"
        };

        Assert.DoesNotContain("glpat-DO-NOT-PRINT-ME", personal.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("glpat-DO-NOT-PRINT-ME-EITHER", impersonation.ToString(), StringComparison.Ordinal);
        Assert.Contains("release-automation", personal.ToString(), StringComparison.Ordinal);
        Assert.Contains("impersonate-me", impersonation.ToString(), StringComparison.Ordinal);
    }
}