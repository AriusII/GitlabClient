using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class CommitStatusesEndpointTests
{
    private const string BaseAddress = "https://gitlab.example/api/v4/";

    private const string Sha = "18f3e63d05582537db6d183d9d557be09e1f90c8";

    [Fact]
    public async Task ListAsync_BuildsCommitStatusesRoute_WithQueryOptions_AndDeserializesEachStatus()
    {
        const string Json = """
                            [
                              {
                                "id": 91,
                                "sha": "18f3e63d05582537db6d183d9d557be09e1f90c8",
                                "ref": "main",
                                "status": "success",
                                "name": "external-ci/build",
                                "target_url": "https://ci.example.com/builds/91",
                                "description": "Build passed",
                                "created_at": "2026-03-01T10:00:00Z",
                                "started_at": "2026-03-01T10:00:05Z",
                                "finished_at": "2026-03-01T10:04:00Z",
                                "allow_failure": false,
                                "coverage": 94.5,
                                "pipeline_id": 1234,
                                "author": {
                                  "id": 7,
                                  "username": "ci-bot",
                                  "name": "CI Bot",
                                  "state": "active",
                                  "web_url": "https://gitlab.example/ci-bot"
                                }
                              },
                              {
                                "id": 90,
                                "sha": "18f3e63d05582537db6d183d9d557be09e1f90c8",
                                "status": "failed"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        CommitStatusesClient repository = new(connection);

        CommitStatusListOptions options = new()
        {
            Ref = "main",
            Stage = "external",
            Name = "external-ci/build",
            PipelineId = 1234,
            All = true,
            OrderBy = "pipeline_id",
            Sort = "desc",
            PerPage = 25
        };

        List<GitLabCommitStatus> statuses = new();
        await foreach (GitLabCommitStatus status in repository.ListAsync(ProjectId.FromId(5), Sha, options,
                           TestContext.Current.CancellationToken))
        {
            statuses.Add(status);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains($"/projects/5/repository/commits/{Sha}/statuses?", requestUri, StringComparison.Ordinal);
        Assert.Contains("ref=main", requestUri, StringComparison.Ordinal);
        Assert.Contains("stage=external", requestUri, StringComparison.Ordinal);
        Assert.Contains("name=external-ci%2Fbuild", requestUri, StringComparison.Ordinal);
        Assert.Contains("pipeline_id=1234", requestUri, StringComparison.Ordinal);
        Assert.Contains("all=true", requestUri, StringComparison.Ordinal);
        Assert.Contains("order_by=pipeline_id", requestUri, StringComparison.Ordinal);
        Assert.Contains("sort=desc", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=25", requestUri, StringComparison.Ordinal);

        Assert.Equal(2, statuses.Count);

        GitLabCommitStatus latest = statuses[0];
        Assert.Equal(91, latest.Id);
        Assert.Equal(Sha, latest.Sha);
        Assert.Equal("main", latest.Ref);
        Assert.Equal("success", latest.Status);
        Assert.Equal("external-ci/build", latest.Name);
        Assert.Equal(new Uri("https://ci.example.com/builds/91"), latest.TargetUrl);
        Assert.Equal("Build passed", latest.Description);
        Assert.Equal(new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero), latest.CreatedAt);
        Assert.Equal(new DateTimeOffset(2026, 3, 1, 10, 0, 5, TimeSpan.Zero), latest.StartedAt);
        Assert.Equal(new DateTimeOffset(2026, 3, 1, 10, 4, 0, TimeSpan.Zero), latest.FinishedAt);
        Assert.False(latest.AllowFailure);
        Assert.Equal(94.5, latest.Coverage);
        Assert.Equal(1234, latest.PipelineId);
        Assert.Equal("ci-bot", latest.Author?.Username);

        Assert.Equal(90, statuses[1].Id);
        Assert.Equal("failed", statuses[1].Status);
        Assert.Null(statuses[1].TargetUrl);
        Assert.Null(statuses[1].Author);
        Assert.Null(statuses[1].Coverage);
    }

    [Fact]
    public async Task ListAsync_EscapesTheShaSegment_AndTheNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        CommitStatusesClient repository = new(connection);

        // The {sha} path parameter also accepts a ref name, which can contain slashes.
        await foreach (GitLabCommitStatus _ in repository.ListAsync(ProjectId.FromPath("gitlab-org/gitlab"),
                           "refs/heads/release/1.0", cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/repository/commits/"
            + "refs%2Fheads%2Frelease%2F1.0/statuses",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_PostsToTheFlatStatusesRoute_WithSerializedBody_AndDeserializesTheStatus()
    {
        const string Json = """
                            {
                              "id": 92,
                              "sha": "18f3e63d05582537db6d183d9d557be09e1f90c8",
                              "ref": "main",
                              "status": "running",
                              "name": "external-ci/build",
                              "target_url": "https://ci.example.com/builds/92",
                              "description": "Build started",
                              "allow_failure": false,
                              "coverage": 0,
                              "pipeline_id": 1234
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

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        CommitStatusesClient repository = new(connection);

        CreateCommitStatusRequest request = new()
        {
            State = "running",
            Ref = "main",
            TargetUrl = new Uri("https://ci.example.com/builds/92"),
            Description = "Build started",
            Name = "external-ci/build",
            Coverage = 94.5,
            PipelineId = 1234
        };

        GitLabCommitStatus status = await repository.CreateAsync(5, Sha, request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);

        // The write route is flat: no "repository", no "commits", and the sha at the very end.
        Assert.Equal($"https://gitlab.example/api/v4/projects/5/statuses/{Sha}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        // The request field is "state"; only the response calls it "status".
        Assert.Contains("\"state\":\"running\"", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("\"status\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"ref\":\"main\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"target_url\":\"https://ci.example.com/builds/92\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"description\":\"Build started\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"name\":\"external-ci/build\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"coverage\":94.5", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"pipeline_id\":1234", sentBody, StringComparison.Ordinal);

        Assert.Equal(92, status.Id);
        Assert.Equal("running", status.Status);
        Assert.Equal("external-ci/build", status.Name);
        Assert.Equal(new Uri("https://ci.example.com/builds/92"), status.TargetUrl);
        Assert.Equal(1234, status.PipelineId);
    }

    [Fact]
    public async Task CreateAsync_OmitsUnsetOptionalFields_AndEscapesBothPathSegments()
    {
        const string Json = """
                            {
                              "id": 93,
                              "sha": "18f3e63d05582537db6d183d9d557be09e1f90c8",
                              "status": "pending"
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

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        CommitStatusesClient repository = new(connection);

        GitLabCommitStatus status = await repository.CreateAsync("gitlab-org/gitlab", "refs/heads/release/1.0",
            new CreateCommitStatusRequest { State = "pending" }, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/statuses/refs%2Fheads%2Frelease%2F1.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"state\":\"pending\"}", sentBody);
        Assert.Equal("pending", status.Status);
        Assert.Null(status.Name);
        Assert.Null(status.Coverage);
    }

    [Fact]
    public async Task CreateAsync_SendsTheContextAliasWhenItIsUsedInsteadOfName()
    {
        const string Json = """
                            {
                              "id": 94,
                              "sha": "18f3e63d05582537db6d183d9d557be09e1f90c8",
                              "status": "success",
                              "name": "coverage"
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

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        CommitStatusesClient repository = new(connection);

        GitLabCommitStatus status = await repository.CreateAsync(5, Sha,
            new CreateCommitStatusRequest { State = "success", Context = "coverage" },
            TestContext.Current.CancellationToken);

        Assert.Equal("{\"state\":\"success\",\"context\":\"coverage\"}", sentBody);
        Assert.Equal("coverage", status.Name);
    }

    [Fact]
    public async Task ListAsync_OnNotFoundResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Commit Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        CommitStatusesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(async () =>
        {
            await foreach (GitLabCommitStatus _ in repository
                               .ListAsync(5, "deadbeef", cancellationToken: TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
            }
        });

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Commit Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_OnValidationError_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "state": ["is not included in the list"] } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        CommitStatusesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateAsync(5, Sha, new CreateCommitStatusRequest { State = "bogus" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }
}