using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class PipelineSchedulesRepositoryTests
{
    /// <summary>
    ///     The list shape (<c>APIEntitiesCiPipelineSchedule</c>): no <c>last_pipeline</c>, which is what makes
    ///     that member nullable on the DTO.
    /// </summary>
    private const string ScheduleSummaryJson = """
                                               {
                                                 "id": 13,
                                                 "description": "Test schedule pipeline",
                                                 "ref": "refs/heads/main",
                                                 "cron": "* * * * *",
                                                 "cron_timezone": "Asia/Tokyo",
                                                 "next_run_at": "2017-05-19T13:41:00.000Z",
                                                 "active": true,
                                                 "created_at": "2017-05-19T13:31:08.849Z",
                                                 "updated_at": "2017-05-19T13:40:17.727Z",
                                                 "owner": {
                                                   "id": 1,
                                                   "username": "root",
                                                   "name": "Administrator",
                                                   "state": "active",
                                                   "avatar_url": "https://gitlab.example/uploads/user/avatar/1/root.png",
                                                   "web_url": "https://gitlab.example/root"
                                                 }
                                               }
                                               """;

    /// <summary>The details shape (<c>APIEntitiesCiPipelineScheduleDetails</c>), which adds <c>last_pipeline</c>.</summary>
    private const string ScheduleDetailsJson = """
                                               {
                                                 "id": 13,
                                                 "description": "Test schedule pipeline",
                                                 "ref": "refs/heads/main",
                                                 "cron": "* * * * *",
                                                 "cron_timezone": "Asia/Tokyo",
                                                 "next_run_at": "2017-05-19T13:41:00.000Z",
                                                 "active": true,
                                                 "created_at": "2017-05-19T13:31:08.849Z",
                                                 "updated_at": "2017-05-19T13:40:17.727Z",
                                                 "owner": {
                                                   "id": 1,
                                                   "username": "root",
                                                   "name": "Administrator",
                                                   "web_url": "https://gitlab.example/root"
                                                 },
                                                 "last_pipeline": {
                                                   "id": 332,
                                                   "iid": 12,
                                                   "project_id": 3,
                                                   "sha": "0e788619d0b5ec17388dffb751e5d0e4c05f4a2e",
                                                   "ref": "main",
                                                   "status": "pending",
                                                   "source": "schedule",
                                                   "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/332"
                                                 }
                                               }
                                               """;

    private static GitLabApiConnection CreateConnection(HttpClient httpClient)
    {
        return new GitLabApiConnection(httpClient);
    }

    [Fact]
    public async Task ListAsync_BuildsPipelineSchedulesRoute_WithQueryOptions_AndDeserializesSchedules()
    {
        string json = $"[{ScheduleSummaryJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        PipelineSchedulesRepository repository = new(CreateConnection(httpClient));

        PipelineScheduleListOptions options = new() { Scope = "active", PerPage = 25 };

        List<GitLabPipelineSchedule> schedules = new();
        await foreach (GitLabPipelineSchedule schedule in repository.ListAsync(1, options,
                           TestContext.Current.CancellationToken))
        {
            schedules.Add(schedule);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/pipeline_schedules", requestUri, StringComparison.Ordinal);
        Assert.Contains("scope=active", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=25", requestUri, StringComparison.Ordinal);

        GitLabPipelineSchedule only = Assert.Single(schedules);
        Assert.Equal(13, only.Id);
        Assert.Equal("Test schedule pipeline", only.Description);
        Assert.Equal("refs/heads/main", only.Ref);
        Assert.Equal("* * * * *", only.Cron);
        Assert.Equal("Asia/Tokyo", only.CronTimezone);
        Assert.True(only.Active);
        Assert.Equal(new DateTimeOffset(2017, 5, 19, 13, 41, 0, TimeSpan.Zero), only.NextRunAt);
        Assert.NotNull(only.Owner);
        Assert.Equal("root", only.Owner!.Username);

        // The list shape carries no last_pipeline; the DTO models both shapes, so it must come back null
        // rather than throwing on the missing member.
        Assert.Null(only.LastPipeline);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        PipelineSchedulesRepository repository = new(CreateConnection(httpClient));

        List<GitLabPipelineSchedule> schedules = new();
        await foreach (GitLabPipelineSchedule schedule in repository.ListAsync("gitlab-org/gitlab",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            schedules.Add(schedule);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/pipeline_schedules",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Empty(schedules);
    }

    [Fact]
    public async Task GetAsync_BuildsScheduleRoute_AndDeserializesLastPipeline()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ScheduleDetailsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        PipelineSchedulesRepository repository = new(CreateConnection(httpClient));

        GitLabPipelineSchedule schedule = await repository.GetAsync(1, 13, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipeline_schedules/13",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(13, schedule.Id);
        Assert.Equal("* * * * *", schedule.Cron);
        Assert.NotNull(schedule.LastPipeline);
        Assert.Equal(332, schedule.LastPipeline!.Id);
        Assert.Equal("pending", schedule.LastPipeline.Status);
        Assert.Equal("schedule", schedule.LastPipeline.Source);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/gitlab/-/pipelines/332"),
            schedule.LastPipeline.WebUrl);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ScheduleDetailsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        PipelineSchedulesRepository repository = new(CreateConnection(httpClient));

        await repository.GetAsync("gitlab-org/gitlab", 13, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/pipeline_schedules/13",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_PostsSerializedBody_AndDeserializesCreatedSchedule()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ScheduleDetailsJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        PipelineSchedulesRepository repository = new(CreateConnection(httpClient));

        CreatePipelineScheduleRequest request = new()
        {
            Description = "Test schedule pipeline",
            Ref = "refs/heads/main",
            Cron = "* * * * *",
            CronTimezone = "Asia/Tokyo",
            Active = true
        };

        GitLabPipelineSchedule schedule = await repository.CreateAsync(42, request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/pipeline_schedules",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"description\":\"Test schedule pipeline\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"ref\":\"refs/heads/main\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"cron\":\"* * * * *\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"cron_timezone\":\"Asia/Tokyo\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"active\":true", sentBody, StringComparison.Ordinal);
        Assert.Equal(13, schedule.Id);
        Assert.Equal("Asia/Tokyo", schedule.CronTimezone);
    }

    [Fact]
    public async Task UpdateAsync_PutsOnlyTheSuppliedFields_AndDeserializesUpdatedSchedule()
    {
        const string Json = """
                            {
                              "id": 13,
                              "description": "Renamed schedule",
                              "ref": "refs/heads/main",
                              "cron": "0 1 * * *",
                              "cron_timezone": "Asia/Tokyo",
                              "active": false
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
        PipelineSchedulesRepository repository = new(CreateConnection(httpClient));

        UpdatePipelineScheduleRequest request = new()
        {
            Description = "Renamed schedule", Cron = "0 1 * * *", Active = false
        };

        GitLabPipelineSchedule schedule = await repository.UpdateAsync(1, 13, request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipeline_schedules/13",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"description\":\"Renamed schedule\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"cron\":\"0 1 * * *\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"active\":false", sentBody, StringComparison.Ordinal);

        // Unset members must be omitted, not sent as null: a null "ref" would be a validation error rather
        // than "leave it alone".
        Assert.DoesNotContain("\"ref\"", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("cron_timezone", sentBody, StringComparison.Ordinal);

        Assert.Equal("Renamed schedule", schedule.Description);
        Assert.False(schedule.Active);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToScheduleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        PipelineSchedulesRepository repository = new(CreateConnection(httpClient));

        await repository.DeleteAsync("gitlab-org/gitlab", 13, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/pipeline_schedules/13",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     GitLab answers <c>play</c> with a bare acknowledgement message rather than an entity, which is why
    ///     the repository goes through the body-less POST overload: this exact payload would fail to
    ///     deserialize into a schedule.
    /// </summary>
    [Fact]
    public async Task PlayAsync_PostsToPlayRoute_WithNoBody_AndIgnoresTheAcknowledgement()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("""{ "message": "201 Created" }""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        PipelineSchedulesRepository repository = new(CreateConnection(httpClient));

        await repository.PlayAsync(1, 13, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipeline_schedules/13/play",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task PlayAsync_OnForbiddenResponse_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        PipelineSchedulesRepository repository = new(CreateConnection(httpClient));

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.PlayAsync(1, 13, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task TakeOwnershipAsync_PostsToTakeOwnershipRoute_AndDeserializesTheNewOwner()
    {
        const string Json = """
                            {
                              "id": 13,
                              "description": "Test schedule pipeline",
                              "ref": "refs/heads/main",
                              "cron": "* * * * *",
                              "active": true,
                              "owner": {
                                "id": 7,
                                "username": "new-owner",
                                "name": "New Owner",
                                "web_url": "https://gitlab.example/new-owner"
                              }
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        PipelineSchedulesRepository repository = new(CreateConnection(httpClient));

        GitLabPipelineSchedule schedule = await repository.TakeOwnershipAsync(1, 13,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipeline_schedules/13/take_ownership",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.NotNull(schedule.Owner);
        Assert.Equal(7, schedule.Owner!.Id);
        Assert.Equal("new-owner", schedule.Owner.Username);
    }

    [Fact]
    public async Task ListPipelinesAsync_BuildsScheduledPipelinesRoute_WithQueryOptions_AndDeserializesPipelines()
    {
        const string Json = """
                            [
                              {
                                "id": 47,
                                "iid": 12,
                                "project_id": 29,
                                "sha": "41c86f8f51a5d4e94d1e1a0f9e0e9a2f8f6f6f6f",
                                "ref": "main",
                                "status": "success",
                                "source": "schedule",
                                "created_at": "2017-05-19T13:41:00.000Z",
                                "updated_at": "2017-05-19T13:45:00.000Z",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/47"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        PipelineSchedulesRepository repository = new(CreateConnection(httpClient));

        PipelineScheduleRunListOptions options = new()
        {
            Scope = "finished", Status = "success", Sort = "desc", PerPage = 50
        };

        List<GitLabPipeline> pipelines = new();
        await foreach (GitLabPipeline pipeline in repository.ListPipelinesAsync("gitlab-org/gitlab", 13, options,
                           TestContext.Current.CancellationToken))
        {
            pipelines.Add(pipeline);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/gitlab-org%2Fgitlab/pipeline_schedules/13/pipelines", requestUri,
            StringComparison.Ordinal);
        Assert.Contains("scope=finished", requestUri, StringComparison.Ordinal);
        Assert.Contains("status=success", requestUri, StringComparison.Ordinal);
        Assert.Contains("sort=desc", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=50", requestUri, StringComparison.Ordinal);

        GitLabPipeline only = Assert.Single(pipelines);
        Assert.Equal(47, only.Id);
        Assert.Equal("success", only.Status);
        Assert.Equal("schedule", only.Source);
        Assert.Equal("main", only.Ref);
    }

    [Fact]
    public async Task GetAsync_OnMissingSchedule_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        PipelineSchedulesRepository repository = new(CreateConnection(httpClient));

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(1, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }

    [Fact]
    public async Task CreateVariableAsync_PostsToTheScheduleVariablesRoute_AndDeserializesTheVariable()
    {
        const string Json = """
                            {
                              "key": "NIGHTLY",
                              "variable_type": "env_var",
                              "value": "true"
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
        PipelineSchedulesRepository repository = new(connection);

        GitLabVariable variable = await repository.CreateVariableAsync(1, 13,
            new CreatePipelineScheduleVariableRequest { Key = "NIGHTLY", Value = "true" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipeline_schedules/13/variables",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"key\":\"NIGHTLY\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"value\":\"true\"", sentBody, StringComparison.Ordinal);

        // variable_type was left unset, so it must be omitted rather than sent as null.
        Assert.DoesNotContain("variable_type", sentBody, StringComparison.Ordinal);
        Assert.Equal("NIGHTLY", variable.Key);
    }

    [Fact]
    public async Task GetVariableAsync_EscapesTheKey_AndEncodesTheNamespacedProjectPath()
    {
        const string Json = """
                            {
                              "key": "NIGHTLY",
                              "variable_type": "file",
                              "value": "config"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelineSchedulesRepository repository = new(connection);

        GitLabVariable variable = await repository.GetVariableAsync("gitlab-org/gitlab", 13, "A B",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/pipeline_schedules/13/variables/A%20B",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("file", variable.VariableType);
    }

    [Fact]
    public async Task UpdateVariableAsync_PutsToTheVariableRoute_WithSerializedBody()
    {
        const string Json = """
                            {
                              "key": "NIGHTLY",
                              "variable_type": "env_var",
                              "value": "false"
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
        PipelineSchedulesRepository repository = new(connection);

        GitLabVariable variable = await repository.UpdateVariableAsync(1, 13, "NIGHTLY",
            new UpdatePipelineScheduleVariableRequest { Value = "false", VariableType = "env_var" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipeline_schedules/13/variables/NIGHTLY",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"value\":\"false\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"variable_type\":\"env_var\"", sentBody, StringComparison.Ordinal);
        Assert.Equal("false", variable.Value);
    }

    [Fact]
    public async Task DeleteVariableAsync_SendsDeleteToTheVariableRoute()
    {
        // GitLab answers 202 and echoes the deleted variable; the transport surfaces no body on DELETE.
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelineSchedulesRepository repository = new(connection);

        await repository.DeleteVariableAsync(1, 13, "NIGHTLY", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipeline_schedules/13/variables/NIGHTLY",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetVariableAsync_OnMissingKey_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Variable Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelineSchedulesRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetVariableAsync(1, 13, "MISSING", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Variable Not Found", exception.Message);
    }
}