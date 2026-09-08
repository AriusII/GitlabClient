using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ProjectMirrorsRepositoryTests
{
    private const string PullMirrorJson = """
                                          {
                                            "id": 27,
                                            "update_status": "finished",
                                            "url": "https://*****:*****@gitlab.example.com/foo/bar.git",
                                            "last_error": null,
                                            "last_update_at": "2020-01-06T17:32:02.823Z",
                                            "last_update_started_at": "2020-01-06T17:31:55.864Z",
                                            "last_successful_update_at": "2020-01-06T17:32:02.823Z",
                                            "enabled": true,
                                            "mirror_trigger_builds": true,
                                            "only_mirror_protected_branches": false,
                                            "mirror_overwrites_diverged_branches": true,
                                            "mirror_branch_regex": null
                                          }
                                          """;

    [Fact]
    public async Task GetAsync_BuildsMirrorPullRoute_AndDeserializesTheMirror()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(PullMirrorJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectMirrorsRepository repository = new(connection);

        GitLabPullMirror mirror = await repository.GetAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/mirror/pull",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(27, mirror.Id);
        Assert.Equal("finished", mirror.UpdateStatus);
        Assert.Equal("https://*****:*****@gitlab.example.com/foo/bar.git", mirror.Url);
        Assert.Null(mirror.LastError);
        Assert.True(mirror.Enabled);
        Assert.True(mirror.MirrorTriggerBuilds);
        Assert.False(mirror.OnlyMirrorProtectedBranches);
        Assert.True(mirror.MirrorOverwritesDivergedBranches);
        Assert.Null(mirror.MirrorBranchRegex);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(PullMirrorJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectMirrorsRepository repository = new(connection);

        await repository.GetAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/mirror/pull",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task StartAsync_WithoutRequest_PostsWithNoBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectMirrorsRepository repository = new(connection);

        await repository.StartAsync(42, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/mirror/pull",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task StartAsync_WithForce_PostsTheRequestBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectMirrorsRepository repository = new(connection);

        TriggerPullMirrorRequest request = new() { Force = true };

        await repository.StartAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/mirror/pull",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal("""{"force":true}""", sentBody);
    }

    [Fact]
    public async Task StartAsync_WithGitHubPullRequestPayload_PostsTheNestedShape()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectMirrorsRepository repository = new(connection);

        TriggerPullMirrorRequest request = new()
        {
            Action = "synchronize",
            PullRequest = new PullMirrorPullRequest
            {
                Number = 7,
                Head = new PullMirrorPullRequestRef
                {
                    Ref = "feature-x",
                    Sha = "abc123",
                    Repo = new PullMirrorPullRequestRepo { FullName = "octocat/hello-world" }
                },
                Base = new PullMirrorPullRequestRef { Ref = "main", Sha = "def456" }
            }
        };

        await repository.StartAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(
            """
            {"action":"synchronize","pull_request":{"number":7,"head":{"ref":"feature-x","sha":"abc123","repo":{"full_name":"octocat/hello-world"}},"base":{"ref":"main","sha":"def456"}}}
            """,
            sentBody);
    }

    [Fact]
    public async Task UpdateAsync_PutsTheSettings_AndDeserializesTheMirror()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(PullMirrorJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectMirrorsRepository repository = new(connection);

        UpdatePullMirrorRequest request = new()
        {
            Enabled = true,
            Url = "https://gitlab.example.com/foo/bar.git",
            AuthUser = "mirror-bot",
            AuthPassword = "s3cr3t",
            OnlyMirrorProtectedBranches = true
        };

        GitLabPullMirror mirror = await repository.UpdateAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/mirror/pull",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """
            {"enabled":true,"url":"https://gitlab.example.com/foo/bar.git","auth_user":"mirror-bot","auth_password":"s3cr3t","only_mirror_protected_branches":true}
            """,
            sentBody);
        Assert.Equal(27, mirror.Id);
        Assert.True(mirror.Enabled);
    }

    [Fact]
    public async Task GetAsync_OnMissingProject_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Project Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ProjectMirrorsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}