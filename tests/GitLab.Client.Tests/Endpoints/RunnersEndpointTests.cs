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

public sealed class RunnersEndpointTests
{
    /// <summary>The list shape (<c>APIEntitiesCiRunner</c>): none of the detail-only members are present.</summary>
    private const string RunnerSummaryJson = """
                                             {
                                               "id": 8,
                                               "description": "test-1-20150125",
                                               "ip_address": "127.0.0.1",
                                               "active": true,
                                               "paused": false,
                                               "is_shared": true,
                                               "runner_type": "instance_type",
                                               "name": "gitlab-runner",
                                               "online": true,
                                               "status": "online",
                                               "job_execution_status": "idle",
                                               "created_at": "2024-01-15T09:00:00.000Z",
                                               "created_by": {
                                                 "id": 1,
                                                 "username": "root",
                                                 "name": "Administrator",
                                                 "web_url": "https://gitlab.example/root"
                                               }
                                             }
                                             """;

    /// <summary>
    ///     The details shape (<c>APIEntitiesCiRunnerDetails</c>), recorded from the live API. The OpenAPI
    ///     document types <c>tag_list</c>, <c>run_untagged</c>, <c>locked</c> and <c>maximum_timeout</c> as
    ///     <c>string</c>; this payload is what GitLab actually sends, and is why the DTO types them as an
    ///     array, two booleans and an integer instead.
    /// </summary>
    private const string RunnerDetailsJson = """
                                             {
                                               "id": 8,
                                               "description": "test-1-20150125",
                                               "ip_address": "127.0.0.1",
                                               "active": true,
                                               "paused": false,
                                               "is_shared": true,
                                               "runner_type": "project_type",
                                               "name": "gitlab-runner",
                                               "online": true,
                                               "status": "online",
                                               "job_execution_status": "running",
                                               "created_at": "2024-01-15T09:00:00.000Z",
                                               "tag_list": ["sast", "docker"],
                                               "run_untagged": true,
                                               "locked": false,
                                               "maximum_timeout": 3600,
                                               "access_level": "ref_protected",
                                               "version": "17.9.0",
                                               "revision": "f9a4f0b1",
                                               "platform": "linux",
                                               "architecture": "amd64",
                                               "contacted_at": "2024-03-01T10:22:31.000Z",
                                               "maintenance_note": "Rebuilt 2024-02-28",
                                               "projects": [
                                                 {
                                                   "id": 1,
                                                   "name": "GitLab",
                                                   "path_with_namespace": "gitlab-org/gitlab",
                                                   "visibility": "public",
                                                   "web_url": "https://gitlab.example/gitlab-org/gitlab"
                                                 }
                                               ],
                                               "groups": [
                                                 {
                                                   "id": 2,
                                                   "name": "GitLab Org",
                                                   "web_url": "https://gitlab.example/groups/gitlab-org"
                                                 }
                                               ]
                                             }
                                             """;

    /// <summary>
    ///     <c>APIEntitiesCiResetTokenResult</c>, shared by all four token-reset endpoints. The token is a
    ///     write-once secret: these responses are the only place one is ever returned.
    /// </summary>
    private const string ResetTokenJson = """
                                          {
                                            "token": "test-registration-token-not-a-real-secret",
                                            "token_expires_at": "2025-06-09T11:12:13.000Z"
                                          }
                                          """;

    private static readonly string[] ExpectedTagList = ["sast", "docker"];

    [Fact]
    public async Task ListAsync_BuildsRunnersRoute_WithQueryOptions_AndDeserializesRunners()
    {
        string json = $"[{RunnerSummaryJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        RunnerListOptions options = new()
        {
            Type = "instance_type",
            Paused = false,
            Status = "online",
            TagList = ["sast", "docker"],
            VersionPrefix = "17.",
            PerPage = 40
        };

        List<GitLabRunner> runners = new();
        await foreach (GitLabRunner runner in repository.ListAsync(options, TestContext.Current.CancellationToken))
        {
            runners.Add(runner);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.StartsWith("https://gitlab.example/api/v4/runners?", requestUri, StringComparison.Ordinal);
        Assert.Contains("type=instance_type", requestUri, StringComparison.Ordinal);
        Assert.Contains("paused=false", requestUri, StringComparison.Ordinal);
        Assert.Contains("status=online", requestUri, StringComparison.Ordinal);
        Assert.Contains("tag_list=sast,docker", requestUri, StringComparison.Ordinal);
        Assert.Contains("version_prefix=17.", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=40", requestUri, StringComparison.Ordinal);

        GitLabRunner only = Assert.Single(runners);
        Assert.Equal(8, only.Id);
        Assert.Equal("test-1-20150125", only.Description);
        Assert.Equal("127.0.0.1", only.IpAddress);
        Assert.True(only.Active);
        Assert.False(only.Paused);
        Assert.True(only.IsShared);
        Assert.Equal("instance_type", only.RunnerType);
        Assert.Equal("online", only.Status);
        Assert.Equal("idle", only.JobExecutionStatus);
        Assert.NotNull(only.CreatedBy);
        Assert.Equal("root", only.CreatedBy!.Username);

        // Detail-only members are absent from the list shape and must come back null.
        Assert.Null(only.TagList);
        Assert.Null(only.MaximumTimeout);
    }

    [Fact]
    public async Task ListAllAsync_BuildsRunnersAllRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{RunnerSummaryJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabRunner> runners = new();
        await foreach (GitLabRunner runner in repository.ListAllAsync(new RunnerListOptions { Status = "stale" },
                           TestContext.Current.CancellationToken))
        {
            runners.Add(runner);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners/all?status=stale",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(runners);
    }

    [Fact]
    public async Task ListAllAsync_WhenNotAnAdministrator_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden - Not authorized" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(async () =>
        {
            await foreach (GitLabRunner _ in repository
                               .ListAllAsync(cancellationToken: TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                // The exception is raised by the first page fetch, before any item is yielded.
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden - Not authorized", exception.Message);
    }

    [Fact]
    public async Task ListProjectRunnersAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{RunnerSummaryJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabRunner> runners = new();
        await foreach (GitLabRunner runner in repository.ListProjectRunnersAsync("gitlab-org/gitlab",
                           new RunnerListOptions { Type = "project_type" }, TestContext.Current.CancellationToken))
        {
            runners.Add(runner);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/runners?type=project_type",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(runners);
    }

    [Fact]
    public async Task ListGroupRunnersAsync_EncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{RunnerSummaryJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabRunner> runners = new();
        await foreach (GitLabRunner runner in repository.ListGroupRunnersAsync("gitlab-org/subgroup",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            runners.Add(runner);
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/runners",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(runners);
    }

    [Fact]
    public async Task GetAsync_DeserializesRunnerDetailAssignments()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RunnerDetailsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabRunner runner = await repository.GetAsync(8, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners/8", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(8, runner.Id);
        Assert.True(runner.Active);
        Assert.Equal("project_type", runner.RunnerType);
        Assert.Equal("running", runner.JobExecutionStatus);
        Assert.Equal(ExpectedTagList, runner.TagList);
        Assert.True(runner.RunUntagged);
        Assert.False(runner.Locked);
        Assert.Equal(3600, runner.MaximumTimeout);
        Assert.Equal("ref_protected", runner.AccessLevel);
        Assert.Equal("17.9.0", runner.Version);
        Assert.Equal("linux", runner.Platform);
        Assert.Equal("amd64", runner.Architecture);
        Assert.Equal(new DateTimeOffset(2024, 3, 1, 10, 22, 31, TimeSpan.Zero), runner.ContactedAt);
        Assert.Equal("Rebuilt 2024-02-28", runner.MaintenanceNote);

        GitLabProject project = Assert.Single(runner.Projects!);
        Assert.Equal(1, project.Id);
        Assert.Equal("gitlab-org/gitlab", project.PathWithNamespace);

        GitLabRunnerGroup group = Assert.Single(runner.Groups!);
        Assert.Equal(2, group.Id);
        Assert.Equal("GitLab Org", group.Name);
        Assert.Equal(new Uri("https://gitlab.example/groups/gitlab-org"), group.WebUrl);
    }

    [Fact]
    public async Task GetAsync_WithIncludeProjectsFalse_SendsTheQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RunnerDetailsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        await repository.GetAsync(8, false, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/runners/8?include_projects=false",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnMissingRunner_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(999, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_PutsSerializedBody_AndDeserializesUpdatedRunner()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(RunnerDetailsJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        UpdateRunnerRequest request = new()
        {
            Description = "test-1-20150125",
            Paused = true,
            TagList = ["sast", "docker"],
            RunUntagged = true,
            Locked = false,
            AccessLevel = "ref_protected",
            MaximumTimeout = 3600,
            MaintenanceNote = "Rebuilt 2024-02-28"
        };

        GitLabRunner runner = await repository.UpdateAsync(8, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners/8", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"paused\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"tag_list\":[\"sast\",\"docker\"]", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"run_untagged\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"locked\":false", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"access_level\":\"ref_protected\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"maximum_timeout\":3600", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"maintenance_note\":\"Rebuilt 2024-02-28\"", sentBody, StringComparison.Ordinal);

        // "active" is deprecated and mutually exclusive with "paused", so it must never be sent.
        Assert.DoesNotContain("\"active\"", sentBody, StringComparison.Ordinal);

        Assert.Equal(3600, runner.MaximumTimeout);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToRunnerRoute_AndDiscardsTheEchoedRunner()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RunnerDetailsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteAsync(8, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners/8", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task AssignToProjectAsync_PostsRunnerId_AndDeserializesAssignedRunner()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(RunnerSummaryJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabRunner runner = await repository.AssignToProjectAsync("gitlab-org/gitlab",
            new AssignRunnerRequest { RunnerId = 8 }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/runners",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"runner_id\":8}", sentBody);
        Assert.Equal(8, runner.Id);
    }

    [Fact]
    public async Task UnassignFromProjectAsync_SendsDeleteToProjectRunnerRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RunnerSummaryJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        await repository.UnassignFromProjectAsync("gitlab-org/gitlab", 8, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/runners/8",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     GitLab refuses to unassign a runner from the project that owns it; the documented alternative is
    ///     deleting the runner. That refusal is a 400, so it must surface as a validation failure.
    /// </summary>
    [Fact]
    public async Task UnassignFromProjectAsync_OnOwnerProject_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": "400 Bad Request - Runner is the owner of the project" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.UnassignFromProjectAsync(1, 8, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("400 Bad Request - Runner is the owner of the project", exception.Message);
    }

    [Fact]
    public async Task ListJobsAsync_BuildsRunnerJobsRoute_WithQueryOptions_AndDeserializesJobs()
    {
        const string Json = """
                            [
                              {
                                "id": 2,
                                "status": "running",
                                "stage": "test",
                                "name": "test",
                                "ref": "main",
                                "tag": false,
                                "duration": 0.166,
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/2",
                                "user": {
                                  "id": 1,
                                  "username": "root",
                                  "name": "Administrator",
                                  "web_url": "https://gitlab.example/root"
                                }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        RunnerJobListOptions options = new()
        {
            Status = "running",
            OrderBy = "id",
            Sort = "desc",
            SystemId = "s_c2b4d3f1a5e6",
            PerPage = 10
        };

        List<GitLabJob> jobs = new();
        await foreach (GitLabJob job in repository.ListJobsAsync(8, options, TestContext.Current.CancellationToken))
        {
            jobs.Add(job);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.StartsWith("https://gitlab.example/api/v4/runners/8/jobs?", requestUri, StringComparison.Ordinal);
        Assert.Contains("status=running", requestUri, StringComparison.Ordinal);
        Assert.Contains("order_by=id", requestUri, StringComparison.Ordinal);
        Assert.Contains("sort=desc", requestUri, StringComparison.Ordinal);
        Assert.Contains("system_id=s_c2b4d3f1a5e6", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=10", requestUri, StringComparison.Ordinal);

        GitLabJob only = Assert.Single(jobs);
        Assert.Equal(2, only.Id);
        Assert.Equal("running", only.Status);
        Assert.Equal("test", only.Name);
        Assert.Equal(0.166, only.Duration);
        Assert.NotNull(only.User);
        Assert.Equal("root", only.User!.Username);
    }

    [Fact]
    public async Task ListManagersAsync_BuildsRunnerManagersRoute_AndDeserializesManagers()
    {
        const string Json = """
                            [
                              {
                                "id": 1,
                                "system_id": "s_89e5e9956577",
                                "version": "17.9.0",
                                "revision": "f9a4f0b1",
                                "platform": "linux",
                                "architecture": "amd64",
                                "created_at": "2024-01-15T09:00:00.000Z",
                                "contacted_at": "2024-03-01T10:22:31.000Z",
                                "ip_address": "127.0.0.1",
                                "status": "online",
                                "job_execution_status": "idle"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabRunnerManager> managers = new();
        await foreach (GitLabRunnerManager manager in repository.ListManagersAsync(8,
                           TestContext.Current.CancellationToken))
        {
            managers.Add(manager);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners/8/managers",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabRunnerManager only = Assert.Single(managers);
        Assert.Equal(1, only.Id);
        Assert.Equal("s_89e5e9956577", only.SystemId);
        Assert.Equal("17.9.0", only.Version);
        Assert.Equal("linux", only.Platform);
        Assert.Equal("amd64", only.Architecture);
        Assert.Equal("127.0.0.1", only.IpAddress);
        Assert.Equal("online", only.Status);
        Assert.Equal("idle", only.JobExecutionStatus);
        Assert.Equal(new DateTimeOffset(2024, 3, 1, 10, 22, 31, TimeSpan.Zero), only.ContactedAt);
    }

    [Fact]
    public async Task ListProjectsAsync_BuildsRunnerProjectsRoute_AndDeserializesProjects()
    {
        const string Json = """
                            [
                              {
                                "id": 1,
                                "name": "GitLab",
                                "path_with_namespace": "gitlab-org/gitlab",
                                "description": "The one",
                                "visibility": "public",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab",
                                "default_branch": "main",
                                "star_count": 12
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabProject> projects = new();
        await foreach (GitLabProject project in repository.ListProjectsAsync(8,
                           TestContext.Current.CancellationToken))
        {
            projects.Add(project);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners/8/projects",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabProject only = Assert.Single(projects);
        Assert.Equal(1, only.Id);
        Assert.Equal("gitlab-org/gitlab", only.PathWithNamespace);
    }

    [Fact]
    public async Task ResetRegistrationTokenAsync_PostsToTheInstanceRoute_AndReturnsTheSecret()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(ResetTokenJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerToken token =
            await repository.ResetRegistrationTokenAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners/reset_registration_token",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("test-registration-token-not-a-real-secret", token.Token);
        Assert.Equal(new DateTimeOffset(2025, 6, 9, 11, 12, 13, TimeSpan.Zero), token.TokenExpiresAt);
    }

    [Fact]
    public async Task ResetProjectRegistrationTokenAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(ResetTokenJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerToken token = await repository.ResetProjectRegistrationTokenAsync("gitlab-org/gitlab",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/runners/reset_registration_token",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("test-registration-token-not-a-real-secret", token.Token);
    }

    [Fact]
    public async Task ResetGroupRegistrationTokenAsync_EncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(ResetTokenJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerToken token = await repository.ResetGroupRegistrationTokenAsync("gitlab-org/subgroup",
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/runners/reset_registration_token",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("test-registration-token-not-a-real-secret", token.Token);
    }

    /// <summary>A runner token with no expiry comes back as an explicit <c>null</c>, which must not throw.</summary>
    [Fact]
    public async Task ResetAuthenticationTokenAsync_PostsToTheRunnerRoute_AndToleratesANullExpiry()
    {
        const string Json = """{ "token": "test-auth-token-not-a-real-secret", "token_expires_at": null }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerToken token =
            await repository.ResetAuthenticationTokenAsync(8, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners/8/reset_authentication_token",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("test-auth-token-not-a-real-secret", token.Token);
        Assert.Null(token.TokenExpiresAt);
    }

    /// <summary>Resetting a registration token is an owner/administrator action; anything less is a 403.</summary>
    [Fact]
    public async Task ResetRegistrationTokenAsync_WhenNotPermitted_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.ResetRegistrationTokenAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_PostsLegacyRegistrationPayload_AndReturnsTheWriteOnceCredential()
    {
        const string Json = """{ "id": 42, "token": "glrt-registered", "token_expires_at": null }""";
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
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerRegistration registration = await repository.RegisterAsync(
            new RegisterRunnerRequest
            {
                Token = "registration-token",
                Description = "linux runner",
                Paused = false,
                TagList = ["docker", "linux"],
                Info = new GitLabRunnerMetadata { Name = "runner-01", Version = "18.7.0", Architecture = "amd64" }
            }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"token\":\"registration-token\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"paused\":false", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"tag_list\":[\"docker\",\"linux\"]", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"info\":{\"name\":\"runner-01\",\"version\":\"18.7.0\",\"architecture\":\"amd64\"}",
            sentBody, StringComparison.Ordinal);
        Assert.Equal(42, registration.Id);
        Assert.Equal("glrt-registered", registration.Token);
        Assert.Null(registration.TokenExpiresAt);
    }

    [Fact]
    public async Task UnregisterAsync_SendsAuthenticationTokenAsAnEscapedQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        await repository.UnregisterAsync("glrt-token+/=", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners?token=glrt-token%2B%2F%3D",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RemoveManagerAsync_SendsBothRunnerProtocolCredentials()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        await repository.RemoveManagerAsync("glrt-token", "s_c2b4d3f1a5e6", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners/managers?token=glrt-token&system_id=s_c2b4d3f1a5e6",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task VerifyAsync_PostsTokenAndSystemId_AndReturnsRegistrationDetails()
    {
        const string Json = """{ "id": 42, "token": "glrt-verified", "token_expires_at": null }""";
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
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerRegistration registration = await repository.VerifyAsync(
            new VerifyRunnerRequest { Token = "glrt-token", SystemId = "s_c2b4d3f1a5e6" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners/verify", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"token\":\"glrt-token\",\"system_id\":\"s_c2b4d3f1a5e6\"}", sentBody);
        Assert.Equal(42, registration.Id);
        Assert.Equal("glrt-verified", registration.Token);
    }

    [Fact]
    public async Task ResetAuthenticationTokenAsync_WithCurrentTokenPostsRunnerProtocolRequest()
    {
        const string Json = """{ "token": "glrt-rotated", "token_expires_at": null }""";
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
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerToken token = await repository.ResetAuthenticationTokenAsync(
            new ResetRunnerAuthenticationTokenRequest { Token = "glrt-current" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners/reset_authentication_token",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"token\":\"glrt-current\"}", sentBody);
        Assert.Equal("glrt-rotated", token.Token);
        Assert.Null(token.TokenExpiresAt);
    }

    [Fact]
    public async Task DiscoverJobRouterAsync_GetsTheSecureWebSocketEndpoint()
    {
        const string Json = """{ "server_url": "wss://kas.gitlab.example/-/kubernetes-agent/" }""";
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnersClient repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerRouterDiscovery discovery =
            await repository.DiscoverJobRouterAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runners/router/discovery",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(new Uri("wss://kas.gitlab.example/-/kubernetes-agent/"), discovery.ServerUrl);
    }
}