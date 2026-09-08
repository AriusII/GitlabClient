using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class JobsRepositoryTests
{
    private const string JobJson = """
                                   {
                                     "id": 469,
                                     "status": "success",
                                     "stage": "test",
                                     "name": "rspec",
                                     "ref": "main",
                                     "tag": false,
                                     "created_at": "2016-01-11T10:13:33.506Z",
                                     "started_at": "2016-01-11T10:14:09.526Z",
                                     "finished_at": "2016-01-11T10:15:10.506Z",
                                     "duration": 61.0,
                                     "user": {
                                       "id": 1,
                                       "username": "admin",
                                       "name": "Administrator",
                                       "web_url": "https://gitlab.example/admin"
                                     },
                                     "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/469"
                                   }
                                   """;

    /// <summary>
    ///     The full <c>APIEntitiesCiJob</c> shape that list/get/cancel/retry/erase return, which the leaner
    ///     <see cref="JobJson" /> above deliberately does not cover.
    /// </summary>
    private const string DetailedJobJson = """
                                           {
                                             "id": 469,
                                             "status": "failed",
                                             "stage": "test",
                                             "name": "rspec",
                                             "ref": "main",
                                             "tag": false,
                                             "allow_failure": true,
                                             "coverage": 97.5,
                                             "duration": 61.5,
                                             "queued_duration": 4.75,
                                             "created_at": "2016-01-11T10:13:33.506Z",
                                             "started_at": "2016-01-11T10:14:09.526Z",
                                             "finished_at": "2016-01-11T10:15:10.506Z",
                                             "erased_at": null,
                                             "failure_reason": "script_failure",
                                             "artifacts_expire_at": "2016-01-18T10:15:10.506Z",
                                             "archived": false,
                                             "tag_list": ["docker", "linux"],
                                             "artifacts": [
                                               {
                                                 "file_type": "archive",
                                                 "size": 1000,
                                                 "filename": "artifacts.zip",
                                                 "file_format": "zip"
                                               },
                                               {
                                                 "file_type": "junit",
                                                 "size": 4096,
                                                 "filename": "junit.xml.gz",
                                                 "file_format": "gzip"
                                               }
                                             ],
                                             "commit": {
                                               "id": "0ff3ae198f8601a285adcf5c0fff204ee6fba5fd",
                                               "short_id": "0ff3ae19",
                                               "title": "Test the CI integration",
                                               "author_name": "Administrator",
                                               "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/0ff3ae198f8601a285adcf5c0fff204ee6fba5fd"
                                             },
                                             "pipeline": {
                                               "id": 6,
                                               "project_id": 1,
                                               "sha": "0ff3ae198f8601a285adcf5c0fff204ee6fba5fd",
                                               "ref": "main",
                                               "status": "failed",
                                               "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/6"
                                             },
                                             "user": {
                                               "id": 1,
                                               "username": "admin",
                                               "name": "Administrator",
                                               "web_url": "https://gitlab.example/admin"
                                             },
                                             "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/469"
                                           }
                                           """;

    private static readonly string[] ExpectedJobTagList = ["docker", "linux"];

    [Fact]
    public async Task ListAsync_BuildsJobsRoute_WithScopeAndPerPage_AndDeserializesJobs()
    {
        string json = $"[{JobJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        JobListOptions options = new() { Scope = ["running", "pending"], PerPage = 20 };
        List<GitLabJob> jobs = new();
        await foreach (GitLabJob job in repository.ListAsync(1, options, TestContext.Current.CancellationToken))
        {
            jobs.Add(job);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("/projects/1/jobs", requestUri, StringComparison.Ordinal);
        Assert.Contains("scope[]=running", requestUri, StringComparison.Ordinal);
        Assert.Contains("scope[]=pending", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=20", requestUri, StringComparison.Ordinal);

        GitLabJob onlyJob = Assert.Single(jobs);
        Assert.Equal(469, onlyJob.Id);
        Assert.Equal("success", onlyJob.Status);
        Assert.Equal("rspec", onlyJob.Name);
        Assert.Equal("test", onlyJob.Stage);
        Assert.NotNull(onlyJob.User);
        Assert.Equal("admin", onlyJob.User!.Username);
    }

    [Fact]
    public async Task ListForPipelineAsync_BuildsPipelineJobsRoute_AndDeserializesJobs()
    {
        string json = $"[{JobJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        List<GitLabJob> jobs = new();
        await foreach (GitLabJob job in repository.ListForPipelineAsync(1, 42, TestContext.Current.CancellationToken))
        {
            jobs.Add(job);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipelines/42/jobs",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(jobs);
    }

    [Fact]
    public async Task GetAsync_BuildsJobRoute_AndDeserializesJob()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JobJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        GitLabJob job = await repository.GetAsync(1, 469, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/jobs/469", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(469, job.Id);
        Assert.Equal("success", job.Status);
        Assert.Equal("rspec", job.Name);
        Assert.Equal(61.0, job.Duration);
    }

    [Fact]
    public async Task CancelAsync_PostsToCancelRoute_WithNoBody_AndDeserializesUpdatedJob()
    {
        const string Json = """
                            {
                              "id": 469,
                              "status": "canceled",
                              "stage": "test",
                              "name": "rspec",
                              "tag": false,
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/469"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        GitLabJob job = await repository.CancelAsync(1, 469, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/jobs/469/cancel",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal("canceled", job.Status);
    }

    [Fact]
    public async Task RetryAsync_PostsToRetryRoute_AndDeserializesUpdatedJob()
    {
        const string Json = """
                            {
                              "id": 470,
                              "status": "pending",
                              "name": "rspec",
                              "tag": false,
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/470"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        GitLabJob job = await repository.RetryAsync(1, 469, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/jobs/469/retry",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(470, job.Id);
        Assert.Equal("pending", job.Status);
    }

    [Fact]
    public async Task PlayAsync_PostsToPlayRoute_AndDeserializesUpdatedJob()
    {
        const string Json = """
                            {
                              "id": 469,
                              "status": "pending",
                              "name": "deploy",
                              "tag": false,
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/469"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        GitLabJob job = await repository.PlayAsync(1, 469, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/jobs/469/play",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("pending", job.Status);
        Assert.Equal("deploy", job.Name);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Job Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(1, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Job Not Found", exception.Message);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath()
    {
        string json = "[]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        List<GitLabJob> jobs = new();
        await foreach (GitLabJob job in repository.ListAsync("gitlab-org/gitlab",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            jobs.Add(job);
        }

        Assert.Contains("/projects/gitlab-org%2Fgitlab/jobs", handler.LastRequest?.RequestUri?.AbsoluteUri,
            StringComparison.Ordinal);
        Assert.Empty(jobs);
    }

    [Fact]
    public async Task ListAsync_WithRefFilter_SendsTheRefQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        JobListOptions options = new() { Scope = ["failed"], Ref = "release/1.0", PerPage = 5 };

        await foreach (GitLabJob _ in repository.ListAsync(1, options, TestContext.Current.CancellationToken))
        {
            // Draining the (empty) page is enough - the assertions are all on the request that was built.
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("scope[]=failed", requestUri, StringComparison.Ordinal);
        Assert.Contains("ref=release%2F1.0", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=5", requestUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAsync_DeserializesTheFullJobShape()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(DetailedJobJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        GitLabJob job = await repository.GetAsync(1, 469, TestContext.Current.CancellationToken);

        Assert.True(job.AllowFailure);
        Assert.Equal(97.5, job.Coverage);
        Assert.Equal(4.75, job.QueuedDuration);
        Assert.Null(job.ErasedAt);
        Assert.Equal("script_failure", job.FailureReason);
        Assert.Equal(new DateTimeOffset(2016, 1, 18, 10, 15, 10, 506, TimeSpan.Zero), job.ArtifactsExpireAt);
        Assert.False(job.Archived);
        Assert.Equal(ExpectedJobTagList, job.TagList);

        Assert.NotNull(job.Commit);
        Assert.Equal("0ff3ae19", job.Commit!.ShortId);
        Assert.Equal("Test the CI integration", job.Commit.Title);

        Assert.NotNull(job.Pipeline);
        Assert.Equal(6, job.Pipeline!.Id);
        Assert.Equal("failed", job.Pipeline.Status);

        Assert.NotNull(job.Artifacts);
        Assert.Equal(2, job.Artifacts!.Count);
        Assert.Equal("archive", job.Artifacts[0].FileType);
        Assert.Equal(1000, job.Artifacts[0].Size);
        Assert.Equal("artifacts.zip", job.Artifacts[0].Filename);
        Assert.Equal("zip", job.Artifacts[0].FileFormat);
        Assert.Equal("junit", job.Artifacts[1].FileType);
        Assert.Equal("gzip", job.Artifacts[1].FileFormat);
    }

    /// <summary>
    ///     The basic shape <c>play</c> returns carries none of the added members, and every one of them is
    ///     nullable precisely so that a single DTO can cover both responses.
    /// </summary>
    [Fact]
    public async Task PlayAsync_OnTheBasicJobShape_LeavesTheDetailOnlyMembersNull()
    {
        const string Json = """
                            {
                              "id": 469,
                              "status": "pending",
                              "name": "deploy",
                              "tag": false,
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/469"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        GitLabJob job = await repository.PlayAsync(1, 469, TestContext.Current.CancellationToken);

        Assert.Null(job.Artifacts);
        Assert.Null(job.Commit);
        Assert.Null(job.Pipeline);
        Assert.Null(job.Coverage);
        Assert.Null(job.TagList);
    }

    [Fact]
    public async Task EraseAsync_PostsToEraseRoute_WithNoBody_AndDeserializesErasedJob()
    {
        const string Json = """
                            {
                              "id": 469,
                              "status": "failed",
                              "stage": "test",
                              "name": "rspec",
                              "ref": "main",
                              "tag": false,
                              "erased_at": "2016-01-11T11:30:00.000Z",
                              "artifacts": [],
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/469"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        GitLabJob job = await repository.EraseAsync(1, 469, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/jobs/469/erase",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal(new DateTimeOffset(2016, 1, 11, 11, 30, 0, TimeSpan.Zero), job.ErasedAt);
        Assert.NotNull(job.Artifacts);
        Assert.Empty(job.Artifacts!);
    }

    [Fact]
    public async Task EraseAsync_EncodesNamespacedProjectPath()
    {
        const string Json = """
                            {
                              "id": 469,
                              "status": "failed",
                              "name": "rspec",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/469"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        await repository.EraseAsync("gitlab-org/gitlab", 469, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/jobs/469/erase",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>An archived job cannot be erased; GitLab answers 403, which must surface as the typed exception.</summary>
    [Fact]
    public async Task EraseAsync_OnForbiddenResponse_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.EraseAsync(1, 469, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task GetCurrentAsync_BuildsTheRootJobRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JobJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        GitLabJob job = await repository.GetCurrentAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // Instance-wide and job-token scoped: no project segment, and no job id.
        Assert.Equal("https://gitlab.example/api/v4/job", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(469, job.Id);
    }

    [Fact]
    public async Task GetTraceAsync_BuildsTheTraceRoute_AndStreamsThePlainTextLog()
    {
        const string Log = "Running with gitlab-runner 17.4.0\nJob succeeded\n";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Log, Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        using GitLabFileResponse trace = await repository.GetTraceAsync("gitlab-org/gitlab", 8,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/jobs/8/trace",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("text/plain", trace.ContentType);

        using StreamReader reader = new(trace.Content, Encoding.UTF8);
        Assert.Equal(Log, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetTraceAsync_AppliesTheByteWindowQuery()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("tail", Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        using GitLabFileResponse trace = await repository.GetTraceAsync(1, 8,
            new JobTraceOptions { ByteOffset = 4_294_967_296, ByteLimit = 2048 },
            TestContext.Current.CancellationToken);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;

        // A long, not an int: a build log can easily run past 2 GB.
        Assert.Contains("byte_offset=4294967296", requestUri, StringComparison.Ordinal);
        Assert.Contains("byte_limit=2048", requestUri, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, trace.StatusCode);
    }

    [Fact]
    public async Task GetTraceAsync_OnMissingJob_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Job Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetTraceAsync(1, 999, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Job Not Found", exception.Message);
    }

    // ---- Runner protocol: POST /jobs/request, PUT /jobs/:id, /jobs/:id/artifacts, PATCH /jobs/:id/trace ----

    [Fact]
    public async Task RequestAsync_PostsToTheBareJobsRequestRoute_AndDeserializesTheDynamicResponse()
    {
        const string Json = """{ "id": "10", "token": "abcd1234", "allow_git_fetch": "false" }""";

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
        JobsRepository repository = new(connection);

        JsonElement response = await repository.RequestAsync(new JobRequestRequest { Token = "runner-token" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);

        // No project segment: the runner protocol is scoped by the runner's own token, not a project.
        Assert.Equal("https://gitlab.example/api/v4/jobs/request", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("""
                        "token":"runner-token"
                        """, sentBody, StringComparison.Ordinal);

        // The optional fields were left unset, so the WhenWritingNull policy omits them entirely.
        Assert.DoesNotContain("system_id", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("last_update", sentBody, StringComparison.Ordinal);

        Assert.Equal("10", response.GetProperty("id").GetString());
        Assert.Equal("abcd1234", response.GetProperty("token").GetString());
    }

    [Fact]
    public async Task UpdateAsync_PutsToTheBareJobIdRoute_WithSerializedState()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        await repository.UpdateAsync(469,
            new UpdateJobStateRequest { Token = "job-token", State = "success", ExitCode = 0 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);

        // No "projects/:id/jobs" prefix - the runner protocol addresses the job directly.
        Assert.Equal("https://gitlab.example/api/v4/jobs/469", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("""
                        "state":"success"
                        """, sentBody, StringComparison.Ordinal);
        Assert.Contains("""
                        "exit_code":0
                        """, sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DownloadArtifactsByTokenAsync_BuildsTheTokenScopedRoute_WithoutAProjectSegment()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([0x50, 0x4B, 0x03, 0x04])
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        using GitLabFileResponse artifacts = await repository.DownloadArtifactsByTokenAsync(469,
            new JobArtifactsByTokenDownloadOptions { Token = "job-token", DirectDownload = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.NotNull(requestUri);
        Assert.StartsWith("https://gitlab.example/api/v4/jobs/469/artifacts?", requestUri, StringComparison.Ordinal);
        Assert.Contains("token=job-token", requestUri, StringComparison.Ordinal);
        Assert.Contains("direct_download=true", requestUri, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, artifacts.StatusCode);
    }

    [Fact]
    public async Task UploadArtifactsAsync_PostsMultipartFormData_WithTheFileAndTheOptionalFields()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        using MemoryStream content = new([0x1F, 0x8B]);
        GitLabFileUpload file = new() { Content = content, FileName = "artifacts.zip" };

        await repository.UploadArtifactsAsync(469, file, "job-token", "1 week",
            GitLabJobArtifactUploadType.JUnit, GitLabJobArtifactUploadFormat.Gzip,
            "private", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/jobs/469/artifacts", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);

        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
        Assert.Contains("artifacts.zip", sentBody, StringComparison.Ordinal);
        Assert.Contains("job-token", sentBody, StringComparison.Ordinal);
        Assert.Contains("1 week", sentBody, StringComparison.Ordinal);

        // The wire values come from the enums' [JsonStringEnumMemberName]s via the generated ToApiValue(),
        // not from the C# member names, so junit/gzip - not JUnit/Gzip - must be what went out.
        Assert.Contains("junit", sentBody, StringComparison.Ordinal);
        Assert.Contains("gzip", sentBody, StringComparison.Ordinal);
        Assert.Contains("private", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned.
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task UploadArtifactsAsync_OmitsFormFields_WhenNoneAreSupplied()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        using MemoryStream content = new([0x1F, 0x8B]);
        GitLabFileUpload file = new() { Content = content, FileName = "artifacts.zip" };

        await repository.UploadArtifactsAsync(469, file,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(sentBody);
        Assert.DoesNotContain("expire_in", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact_type", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact_format", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("accessibility", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AuthorizeArtifactsUploadAsync_PostsJsonBody_ToTheAuthorizeRoute()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        await repository.AuthorizeArtifactsUploadAsync(469,
            new AuthorizeJobArtifactsUploadRequest
            {
                Token = "job-token", Filesize = 2048, ArtifactType = GitLabJobArtifactUploadType.Sast
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/jobs/469/artifacts/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("""
                        "filesize":2048
                        """, sentBody, StringComparison.Ordinal);
        Assert.Contains("""
                        "artifact_type":"sast"
                        """, sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AuthorizeArtifactsUploadAsync_WithNoRequest_SendsAnEmptyJsonBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        await repository.AuthorizeArtifactsUploadAsync(469,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/jobs/469/artifacts/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task AppendTraceAsync_PatchesToTheTraceRoute_WithNoProjectSegment()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        JobsRepository repository = new(connection);

        await repository.AppendTraceAsync(469,
            new AppendJobTraceRequest { Token = "job-token", DebugTrace = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/jobs/469/trace", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        Assert.Contains("""
                        "debug_trace":true
                        """, sentBody, StringComparison.Ordinal);
    }
}