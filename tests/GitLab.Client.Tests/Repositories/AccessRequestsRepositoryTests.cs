using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class AccessRequestsRepositoryTests
{
    private const string AccessRequestJson = """
                                             {
                                               "id": 1,
                                               "username": "raymond_smith",
                                               "name": "Raymond Smith",
                                               "state": "active",
                                               "locked": false,
                                               "public_email": "raymond@example.com",
                                               "avatar_url": "https://gitlab.example/uploads/-/system/user/avatar/1/avatar.png",
                                               "avatar_path": "/uploads/-/system/user/avatar/1/avatar.png",
                                               "web_url": "https://gitlab.example/raymond_smith",
                                               "requested_at": "2012-10-22T14:13:35Z"
                                             }
                                             """;

    private const string MemberJson = """
                                      {
                                        "id": 1,
                                        "username": "raymond_smith",
                                        "name": "Raymond Smith",
                                        "state": "active",
                                        "avatar_url": "https://gitlab.example/uploads/-/system/user/avatar/1/avatar.png",
                                        "web_url": "https://gitlab.example/raymond_smith",
                                        "access_level": 20,
                                        "expires_at": "2012-10-22"
                                      }
                                      """;

    [Fact]
    public async Task ListForProjectAsync_BuildsAccessRequestsRoute_AndDeserializesRequesters()
    {
        string json = $"[{AccessRequestJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessRequestsRepository repository = new(connection);

        List<GitLabAccessRequest> requests = new();
        await foreach (GitLabAccessRequest item in
                       repository.ListForProjectAsync(1, TestContext.Current.CancellationToken))
        {
            requests.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/access_requests",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabAccessRequest accessRequest = Assert.Single(requests);
        Assert.Equal(1, accessRequest.Id);
        Assert.Equal("raymond_smith", accessRequest.Username);
        Assert.Equal("Raymond Smith", accessRequest.Name);
        Assert.Equal("active", accessRequest.State);
        Assert.False(accessRequest.Locked);
        Assert.Equal("raymond@example.com", accessRequest.PublicEmail);
        Assert.Equal("/uploads/-/system/user/avatar/1/avatar.png", accessRequest.AvatarPath);
        Assert.Equal(new Uri("https://gitlab.example/raymond_smith"), accessRequest.WebUrl);
        Assert.Equal(new DateTimeOffset(2012, 10, 22, 14, 13, 35, TimeSpan.Zero), accessRequest.RequestedAt);
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
        AccessRequestsRepository repository = new(connection);

        await foreach (GitLabAccessRequest _ in
                       repository.ListForProjectAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/access_requests",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupRoute_AndEncodesNamespacedGroupPath()
    {
        string json = $"[{AccessRequestJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessRequestsRepository repository = new(connection);

        List<GitLabAccessRequest> requests = new();
        await foreach (GitLabAccessRequest item in
                       repository.ListForGroupAsync("parent-group/subgroup", TestContext.Current.CancellationToken))
        {
            requests.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/access_requests",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("raymond_smith", Assert.Single(requests).Username);
    }

    [Fact]
    public async Task RequestForProjectAsync_PostsWithoutABody_AndDeserializesTheAccessRequest()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AccessRequestJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessRequestsRepository repository = new(connection);

        GitLabAccessRequest accessRequest =
            await repository.RequestForProjectAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/access_requests",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal(1, accessRequest.Id);
        Assert.Equal("Raymond Smith", accessRequest.Name);
        Assert.Equal(new DateTimeOffset(2012, 10, 22, 14, 13, 35, TimeSpan.Zero), accessRequest.RequestedAt);
    }

    [Fact]
    public async Task RequestForGroupAsync_PostsWithoutABody_ToTheGroupRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AccessRequestJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessRequestsRepository repository = new(connection);

        GitLabAccessRequest accessRequest =
            await repository.RequestForGroupAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/access_requests",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal("raymond_smith", accessRequest.Username);
    }

    [Fact]
    public async Task ApproveForProjectAsync_PutsAccessLevel_AndDeserializesTheNewMember()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(MemberJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessRequestsRepository repository = new(connection);

        GitLabMember member = await repository.ApproveForProjectAsync(
            "gitlab-org/gitlab",
            1,
            new ApproveAccessRequestRequest { AccessLevel = 20 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/access_requests/1/approve",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal("""{"access_level":20}""", sentBody);

        Assert.Equal(1, member.Id);
        Assert.Equal("raymond_smith", member.Username);
        Assert.Equal(20, member.AccessLevel);
        Assert.Equal(new DateOnly(2012, 10, 22), member.ExpiresAt);
    }

    [Fact]
    public async Task ApproveForProjectAsync_WithoutARequest_SendsAnEmptyJsonObject()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(MemberJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessRequestsRepository repository = new(connection);

        // Omitting the body is how a caller asks for GitLab's default access level (30, Developer).
        await repository.ApproveForProjectAsync(1, 7, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/access_requests/7/approve",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{}", sentBody);
    }

    [Fact]
    public async Task ApproveForGroupAsync_PutsToTheGroupApproveRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(MemberJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessRequestsRepository repository = new(connection);

        GitLabMember member = await repository.ApproveForGroupAsync(
            "parent-group/subgroup",
            1,
            new ApproveAccessRequestRequest { AccessLevel = 30 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/access_requests/1/approve",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"access_level":30}""", sentBody);
        Assert.Equal("Raymond Smith", member.Name);
    }

    [Fact]
    public async Task DenyForProjectAsync_SendsDeleteToTheAccessRequestRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessRequestsRepository repository = new(connection);

        await repository.DenyForProjectAsync("gitlab-org/gitlab", 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/access_requests/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DenyForGroupAsync_SendsDeleteToTheGroupAccessRequestRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessRequestsRepository repository = new(connection);

        await repository.DenyForGroupAsync(9970, 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/access_requests/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ApproveForProjectAsync_OnForbidden_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessRequestsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.ApproveForProjectAsync(1, 7, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task DenyForGroupAsync_OnMissingRequester_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AccessRequestsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DenyForGroupAsync(9970, 404, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }
}