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

public sealed class ProtectedTagsEndpointTests
{
    [Fact]
    public async Task ListAsync_BuildsProtectedTagsRoute_AndDeserializesEachProtectedTag()
    {
        const string Json = """
                            [
                              {
                                "name": "release/*",
                                "create_access_levels": [
                                  {
                                    "id": 41,
                                    "access_level": 40,
                                    "access_level_description": "Maintainers",
                                    "deploy_key_id": 17,
                                    "user_id": 18,
                                    "group_id": 19,
                                    "member_role_id": 20,
                                    "member_role_name": "Release Manager"
                                  }
                                ]
                              },
                              {
                                "name": "v*",
                                "create_access_levels": [
                                  {
                                    "access_level": 0,
                                    "access_level_description": "No one"
                                  }
                                ]
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedTagsClient repository = new(connection);

        ProtectedTagListOptions options = new() { Page = 2, PerPage = 50 };

        List<GitLabProtectedTag> tags = new();
        await foreach (GitLabProtectedTag tag in repository.ListAsync(42, options,
                           TestContext.Current.CancellationToken))
        {
            tags.Add(tag);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/protected_tags?page=2&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(2, tags.Count);
        Assert.Equal("release/*", tags[0].Name);
        GitLabProtectedTagAccessLevel createAccess = Assert.Single(tags[0].CreateAccessLevels!);
        Assert.Equal(41L, createAccess.Id);
        Assert.Equal(40, createAccess.AccessLevel);
        Assert.Equal("Maintainers", createAccess.AccessLevelDescription);
        Assert.Equal(17L, createAccess.DeployKeyId);
        Assert.Equal(18L, createAccess.UserId);
        Assert.Equal(19L, createAccess.GroupId);
        Assert.Equal(20L, createAccess.MemberRoleId);
        Assert.Equal("Release Manager", createAccess.MemberRoleName);
        Assert.Equal("v*", tags[1].Name);
        Assert.Equal(0, Assert.Single(tags[1].CreateAccessLevels!).AccessLevel);
    }

    [Fact]
    public async Task GetAsync_EscapesWildcardTagName_AndDeserializesProtectedTag()
    {
        const string Json = """
                            {
                              "name": "release/*",
                              "create_access_levels": [
                                {
                                  "access_level": 30,
                                  "access_level_description": "Developers + Maintainers"
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
        ProtectedTagsClient repository = new(connection);

        GitLabProtectedTag tag = await repository.GetAsync(42, "release/*", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // Both the '/' and the '*' are percent-encoded; GitLab accepts the escaped wildcard.
        Assert.Equal("https://gitlab.example/api/v4/projects/42/protected_tags/release%2F%2A",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("release/*", tag.Name);
        Assert.Equal(30, Assert.Single(tag.CreateAccessLevels!).AccessLevel);
    }

    [Fact]
    public async Task ListAsync_PreservesTheExistingPositionalCancellationTokenOverload()
    {
        const string Json = """[]""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedTagsClient repository = new(connection);

        await foreach (GitLabProtectedTag _ in repository.ListAsync(42, TestContext.Current.CancellationToken))
        {
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/protected_tags",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath()
    {
        const string Json = """{ "name": "v1.0.0", "create_access_levels": [] }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedTagsClient repository = new(connection);

        GitLabProtectedTag tag =
            await repository.GetAsync("gitlab-org/gitlab", "v1.0.0", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/protected_tags/v1.0.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("v1.0.0", tag.Name);
        Assert.Empty(tag.CreateAccessLevels!);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Protected Tag Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedTagsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(42, "missing", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Protected Tag Not Found", exception.Message);
    }

    [Fact]
    public async Task ProtectAsync_PostsScalarCreateAccessLevel_AndDeserializesTheArrayComingBack()
    {
        const string Json = """
                            {
                              "name": "v*",
                              "create_access_levels": [
                                {
                                  "access_level": 40,
                                  "access_level_description": "Maintainers"
                                }
                              ]
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
        ProtectedTagsClient repository = new(connection);

        ProtectTagRequest request = new() { Name = "v*", CreateAccessLevel = 40 };

        GitLabProtectedTag tag = await repository.ProtectAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/protected_tags",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        // Singular in, plural out: the request carries create_access_level, the response create_access_levels.
        Assert.Contains("\"name\":\"v*\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"create_access_level\":40", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("create_access_levels", sentBody, StringComparison.Ordinal);

        Assert.Equal("v*", tag.Name);
        Assert.Equal(40, Assert.Single(tag.CreateAccessLevels!).AccessLevel);
    }

    [Fact]
    public async Task ProtectAsync_OmitsCreateAccessLevel_WhenNotSpecified()
    {
        const string Json = """{ "name": "v*", "create_access_levels": [] }""";

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
        ProtectedTagsClient repository = new(connection);

        await repository.ProtectAsync(42, new ProtectTagRequest { Name = "v*" },
            TestContext.Current.CancellationToken);

        Assert.Equal("""{"name":"v*"}""", sentBody);
    }

    [Fact]
    public async Task ProtectAsync_SerializesEachSupportedAllowedToCreatePrincipal()
    {
        const string Json = """{ "name": "release/*", "create_access_levels": [] }""";

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
        ProtectedTagsClient repository = new(connection);

        ProtectTagRequest request = new()
        {
            Name = "release/*",
            AllowedToCreate =
            [
                new ProtectedBranchAccessRequest { UserId = 7 },
                new ProtectedBranchAccessRequest { GroupId = 8 },
                new ProtectedBranchAccessRequest { DeployKeyId = 9 },
                new ProtectedBranchAccessRequest { AccessLevel = 30 }
            ]
        };

        await repository.ProtectAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Contains("\"allowed_to_create\":[", sentBody, StringComparison.Ordinal);
        Assert.Contains("{\"user_id\":7}", sentBody, StringComparison.Ordinal);
        Assert.Contains("{\"group_id\":8}", sentBody, StringComparison.Ordinal);
        Assert.Contains("{\"deploy_key_id\":9}", sentBody, StringComparison.Ordinal);
        Assert.Contains("{\"access_level\":30}", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnprotectAsync_EscapesWildcardTagName_AndSendsDeleteRequest()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedTagsClient repository = new(connection);

        await repository.UnprotectAsync("gitlab-org/gitlab", "release/*", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/protected_tags/release%2F%2A",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UnprotectAsync_OnForbiddenResponse_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProtectedTagsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.UnprotectAsync(42, "v*", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }
}