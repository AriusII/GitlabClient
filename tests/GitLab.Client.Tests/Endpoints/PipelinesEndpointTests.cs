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

public sealed class PipelinesEndpointTests
{
    [Fact]
    public async Task GetAsync_BuildsPipelineRoute_AndDeserializesPipeline()
    {
        const string Json = """
                            {
                              "id": 47,
                              "iid": 12,
                              "project_id": 1,
                              "sha": "a91957a858320c0e17f3a0eca7cfacbff50ea29a",
                              "ref": "main",
                              "status": "success",
                              "source": "push",
                              "created_at": "2016-08-11T11:28:34.085Z",
                              "updated_at": "2016-08-11T11:32:35.169Z",
                              "detailed_status": {
                                "icon": "status_success",
                                "text": "passed",
                                "label": "passed",
                                "group": "success",
                                "tooltip": "passed",
                                "has_details": true,
                                "details_path": "/gitlab-org/gitlab/-/pipelines/47",
                                "favicon": "/assets/ci_favicons/favicon_status_success.png",
                                "action": {
                                  "icon": "retry",
                                  "title": "Retry",
                                  "path": "/gitlab-org/gitlab/-/pipelines/47/retry",
                                  "method": "post",
                                  "button_title": "Retry pipeline",
                                  "confirmation_message": "Retry this pipeline?"
                                }
                              },
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/47"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        GitLabPipeline pipeline = await repository.GetAsync(1, 47, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipelines/47",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(47, pipeline.Id);
        Assert.Equal(12, pipeline.Iid);
        Assert.Equal(1, pipeline.ProjectId);
        Assert.Equal("a91957a858320c0e17f3a0eca7cfacbff50ea29a", pipeline.Sha);
        Assert.Equal("main", pipeline.Ref);
        Assert.Equal("success", pipeline.Status);
        Assert.Equal("push", pipeline.Source);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/gitlab/-/pipelines/47"), pipeline.WebUrl);
        Assert.NotNull(pipeline.DetailedStatus);
        Assert.Equal("status_success", pipeline.DetailedStatus!.Icon);
        Assert.True(pipeline.DetailedStatus.HasDetails);
        Assert.NotNull(pipeline.DetailedStatus.Action);
        Assert.Equal("Retry pipeline", pipeline.DetailedStatus.Action!.ButtonTitle);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(1, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath_AndAppliesQueryOptions()
    {
        const string Json = """
                            [
                              {
                                "id": 47,
                                "sha": "a91957a858320c0e17f3a0eca7cfacbff50ea29a",
                                "ref": "main",
                                "status": "success",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/47"
                              },
                              {
                                "id": 48,
                                "sha": "b91957a858320c0e17f3a0eca7cfacbff50ea29a",
                                "ref": "main",
                                "status": "failed",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/48"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        PipelineListOptions options = new()
        {
            Status = "success",
            Ref = "main",
            Sha = "a91957a858320c0e17f3a0eca7cfacbff50ea29a",
            Source = "push",
            Name = "Nightly build",
            Username = "octocat",
            Scope = "finished",
            CreatedAfter = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBefore = new DateTimeOffset(2024, 12, 31, 0, 0, 0, TimeSpan.Zero),
            OrderBy = "id",
            Sort = "desc",
            PerPage = 20
        };

        List<GitLabPipeline> pipelines = new();
        await foreach (GitLabPipeline pipeline in repository.ListAsync("gitlab-org/gitlab", options,
                           TestContext.Current.CancellationToken))
        {
            pipelines.Add(pipeline);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("/projects/gitlab-org%2Fgitlab/pipelines", requestUri);
        Assert.Contains("status=success", requestUri);
        Assert.Contains("ref=main", requestUri);
        Assert.Contains("sha=a91957a858320c0e17f3a0eca7cfacbff50ea29a", requestUri);
        Assert.Contains("source=push", requestUri);
        Assert.Contains("name=Nightly%20build", requestUri);
        Assert.Contains("username=octocat", requestUri);
        Assert.Contains("scope=finished", requestUri);
        Assert.Contains("created_after=2024-01-01T00:00:00Z", requestUri);
        Assert.Contains("created_before=2024-12-31T00:00:00Z", requestUri);
        Assert.Contains("order_by=id", requestUri);
        Assert.Contains("sort=desc", requestUri);
        Assert.Contains("per_page=20", requestUri);
        Assert.Equal(2, pipelines.Count);
        Assert.Equal("success", pipelines[0].Status);
        Assert.Equal("failed", pipelines[1].Status);
    }

    [Fact]
    public async Task CreateAsync_PostsToSingularPipelineRoute_WithSerializedBody_AndDeserializesCreatedPipeline()
    {
        const string Json = """
                            {
                              "id": 61,
                              "sha": "384c444e840a515b23f21915ee5766b87068a70",
                              "ref": "develop",
                              "status": "pending",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/61"
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
        PipelinesClient repository = new(connection);

        CreatePipelineRequest request = new()
        {
            Ref = "develop",
            Variables =
            [
                new GitLabPipelineVariableRequest
                {
                    Key = "UPLOAD_TO_S3",
                    Value = "true",
                    VariableType = GitLabPipelineVariableType.EnvironmentVariable
                }
            ],
            Inputs = new Dictionary<string, GitLabPipelineInputValue>(StringComparer.Ordinal)
            {
                ["environment"] = GitLabPipelineInputValue.Text("production"),
                ["retry_count"] = GitLabPipelineInputValue.Number(2),
                ["dry_run"] = GitLabPipelineInputValue.Flag(false),
                ["regions"] = GitLabPipelineInputValue.Sequence(
                    [GitLabPipelineInputValue.Text("eu-west-1"), GitLabPipelineInputValue.Text("us-east-1")])
            }
        };

        GitLabPipeline pipeline = await repository.CreateAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipeline", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"ref\":\"develop\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"variables\":[{\"key\":\"UPLOAD_TO_S3\",\"value\":\"true\",\"variable_type\":\"env_var\"}]",
            sentBody, StringComparison.Ordinal);
        Assert.Contains("\"environment\":\"production\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"retry_count\":2", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"dry_run\":false", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"regions\":[\"eu-west-1\",\"us-east-1\"]", sentBody, StringComparison.Ordinal);
        Assert.Equal(61, pipeline.Id);
        Assert.Equal("develop", pipeline.Ref);
        Assert.Equal("pending", pipeline.Status);
    }

    [Fact]
    public async Task CancelAsync_PostsToCancelRoute_WithNoBody_AndDeserializesUpdatedPipeline()
    {
        const string Json = """
                            {
                              "id": 47,
                              "sha": "a91957a858320c0e17f3a0eca7cfacbff50ea29a",
                              "ref": "main",
                              "status": "canceled",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/47"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        GitLabPipeline pipeline = await repository.CancelAsync(1, 47, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipelines/47/cancel",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal("canceled", pipeline.Status);
    }

    [Fact]
    public async Task RetryAsync_PostsToRetryRoute_WithNoBody_AndDeserializesUpdatedPipeline()
    {
        const string Json = """
                            {
                              "id": 47,
                              "sha": "a91957a858320c0e17f3a0eca7cfacbff50ea29a",
                              "ref": "main",
                              "status": "running",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/47"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        GitLabPipeline pipeline = await repository.RetryAsync(1, 47, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipelines/47/retry",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("running", pipeline.Status);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToPipelineRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        await repository.DeleteAsync(1, 47, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipelines/47",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.DeleteAsync(1, 47, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task ListForCurrentUserAsync_BuildsTheRootPipelinesRoute_AndAppliesQueryOptions()
    {
        const string Json = """
                            [
                              {
                                "id": 47,
                                "sha": "a91957a858320c0e17f3a0eca7cfacbff50ea29a",
                                "ref": "main",
                                "status": "success",
                                "source": "schedule",
                                "name": "Nightly pipeline",
                                "project": {
                                  "id": 1,
                                  "name": "GitLab",
                                  "description": "GitLab source",
                                  "path": "gitlab",
                                  "path_with_namespace": "gitlab-org/gitlab"
                                },
                                "merge_request": {
                                  "iid": 29,
                                  "title": "Add pipeline metadata",
                                  "web_url": "https://gitlab.example/gitlab-org/gitlab/-/merge_requests/29"
                                },
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/47"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        UserPipelineListOptions options = new()
        {
            Source = "schedule",
            CreatedAfter = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero),
            OrderBy = "created_at",
            Sort = "desc",
            Cursor = "eyJpZCI6IjQ3In0",
            PerPage = 50
        };

        List<GitLabPipeline> pipelines = [];
        await foreach (GitLabPipeline pipeline in repository.ListForCurrentUserAsync(options,
                           TestContext.Current.CancellationToken))
        {
            pipelines.Add(pipeline);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;

        // The endpoint is instance-wide, not project-scoped: no /projects segment at all.
        Assert.StartsWith("https://gitlab.example/api/v4/pipelines?", requestUri, StringComparison.Ordinal);
        Assert.Contains("source=schedule", requestUri, StringComparison.Ordinal);
        Assert.Contains("created_after=2024-01-02T03:04:05Z", requestUri, StringComparison.Ordinal);
        Assert.Contains("order_by=created_at", requestUri, StringComparison.Ordinal);
        Assert.Contains("sort=desc", requestUri, StringComparison.Ordinal);
        Assert.Contains("cursor=eyJpZCI6IjQ3In0", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=50", requestUri, StringComparison.Ordinal);
        GitLabPipeline only = Assert.Single(pipelines);
        Assert.Equal("schedule", only.Source);
        Assert.Equal("Nightly pipeline", only.Name);
        Assert.Equal(1, only.Project?.Id);
        Assert.Equal("GitLab", only.Project?.Name);
        Assert.Equal("gitlab-org/gitlab", only.Project?.PathWithNamespace);
        Assert.Equal(29, only.MergeRequest?.Iid);
        Assert.Equal("Add pipeline metadata", only.MergeRequest?.Title);
        Assert.Equal("https://gitlab.example/gitlab-org/gitlab/-/merge_requests/29",
            only.MergeRequest?.WebUrl?.AbsoluteUri);
    }

    [Fact]
    public async Task GetLatestAsync_BuildsTheLatestRoute_AndEscapesASlashBearingRef()
    {
        const string Json = """
                            {
                              "id": 287,
                              "sha": "b91957a858320c0e17f3a0eca7cfacbff50ea29a",
                              "ref": "release/1.0",
                              "status": "success",
                              "name": "Release build",
                              "before_sha": "0000000000000000000000000000000000000000",
                              "tag": false,
                              "duration": 118,
                              "queued_duration": 2,
                              "coverage": 88.5,
                              "archived": false,
                              "started_at": "2024-01-02T03:04:05.000Z",
                              "finished_at": "2024-01-02T03:06:03.000Z",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/287"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        GitLabPipeline pipeline =
            await repository.GetLatestAsync(1, "release/1.0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipelines/latest?ref=release%2F1.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Release build", pipeline.Name);
        Assert.Equal(118, pipeline.Duration);
        Assert.Equal(88.5, pipeline.Coverage);
        Assert.False(pipeline.Tag);
        Assert.False(pipeline.Archived);
    }

    [Fact]
    public async Task GetLatestAsync_WithoutARef_SendsNoQueryString()
    {
        const string Json = """
                            {
                              "id": 287,
                              "sha": "b91957a858320c0e17f3a0eca7cfacbff50ea29a",
                              "ref": "main",
                              "status": "success",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/287"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        await repository.GetLatestAsync("gitlab-org/gitlab", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/pipelines/latest",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateMetadataAsync_PutsTheNameToTheMetadataRoute_AndReturnsTheRenamedPipeline()
    {
        const string Json = """
                            {
                              "id": 47,
                              "sha": "a91957a858320c0e17f3a0eca7cfacbff50ea29a",
                              "ref": "main",
                              "status": "success",
                              "name": "Nightly build",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/47"
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
        PipelinesClient repository = new(connection);

        GitLabPipeline pipeline = await repository.UpdateMetadataAsync(1, 47,
            new UpdatePipelineMetadataRequest { Name = "Nightly build" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipelines/47/metadata",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"name\":\"Nightly build\"", sentBody, StringComparison.Ordinal);
        Assert.Equal("Nightly build", pipeline.Name);
    }

    [Fact]
    public async Task GetTestReportAsync_BuildsTheTestReportRoute_AndDeserializesSuitesAndCases()
    {
        // execution_time is a float on the wire even though the spec types it as an integer, which is why
        // every duration in the test report DTOs is a double. A JUnit report with sub-second cases would
        // otherwise fail to deserialize on the very first call.
        const string Json = """
                            {
                              "total_time": 5.6,
                              "total_count": 3,
                              "success_count": 2,
                              "failed_count": 1,
                              "skipped_count": 0,
                              "error_count": 0,
                              "test_suites": [
                                {
                                  "name": "rspec",
                                  "total_time": 5.6,
                                  "total_count": 3,
                                  "success_count": 2,
                                  "failed_count": 1,
                                  "skipped_count": 0,
                                  "error_count": 0,
                                  "suite_error": null,
                                  "test_cases": [
                                    {
                                      "status": "failed",
                                      "name": "Security Reports can create an auto-remediation MR",
                                      "classname": "vulnerability_management_spec",
                                      "file": "./spec/test_spec.rb",
                                      "execution_time": 0.009411,
                                      "system_output": "Failure/Error: is_expected.to eq(3)",
                                      "stack_trace": null,
                                      "recent_failures": { "count": 3, "base_branch": "main" },
                                      "attachment_url": "/gitlab-org/gitlab/-/jobs/1/artifacts/file/some/path.png"
                                    }
                                  ]
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
        PipelinesClient repository = new(connection);

        GitLabTestReport report =
            await repository.GetTestReportAsync("gitlab-org/gitlab", 47, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/pipelines/47/test_report",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(5.6, report.TotalTime);
        Assert.Equal(1, report.FailedCount);

        GitLabTestSuite suite = Assert.Single(report.TestSuites!);
        Assert.Equal("rspec", suite.Name);
        Assert.Null(suite.SuiteError);
        Assert.Null(suite.BuildIds);

        GitLabTestCase testCase = Assert.Single(suite.TestCases!);
        Assert.Equal("failed", testCase.Status);
        Assert.Equal("vulnerability_management_spec", testCase.ClassName);
        Assert.Equal(0.009411, testCase.ExecutionTime);
        Assert.Equal("Failure/Error: is_expected.to eq(3)", testCase.SystemOutput);
        Assert.Equal(3, testCase.RecentFailures?.Count);
        Assert.Equal("main", testCase.RecentFailures?.BaseBranch);

        // GitLab sends this one relative to the instance root, so the Uri must not be forced to absolute.
        Assert.Equal("/gitlab-org/gitlab/-/jobs/1/artifacts/file/some/path.png",
            testCase.AttachmentUrl?.OriginalString);
    }

    [Fact]
    public async Task GetTestReportSummaryAsync_BuildsTheSummaryRoute_AndDeserializesTotalsAndBuildIds()
    {
        const string Json = """
                            {
                              "total": {
                                "time": 0.42,
                                "count": 2,
                                "success": 2,
                                "failed": 0,
                                "skipped": 0,
                                "error": 0,
                                "suite_error": null
                              },
                              "test_suites": [
                                {
                                  "name": "rspec",
                                  "total_time": 0.42,
                                  "total_count": 2,
                                  "success_count": 2,
                                  "failed_count": 0,
                                  "skipped_count": 0,
                                  "error_count": 0,
                                  "suite_error": null,
                                  "build_ids": [66004, 66005]
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
        PipelinesClient repository = new(connection);

        GitLabTestReportSummary summary =
            await repository.GetTestReportSummaryAsync(1, 47, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipelines/47/test_report_summary",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(0.42, summary.Total?.Time);
        Assert.Equal(2, summary.Total?.Count);
        Assert.Null(summary.Total?.SuiteError);

        GitLabTestSuite suite = Assert.Single(summary.TestSuites!);
        Assert.Equal(2, suite.BuildIds?.Count);
        Assert.Equal(66004, suite.BuildIds?[0]);
        Assert.Equal(66005, suite.BuildIds?[1]);
        Assert.Null(suite.TestCases);
    }

    [Fact]
    public async Task ListJobsAsync_BuildsThePipelineJobsRoute_AndAppliesPipelineOnlyFilters()
    {
        const string Json = """
                            [
                              {
                                "id": 81,
                                "status": "success",
                                "name": "build",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/81",
                                "pipeline": {
                                  "id": 47,
                                  "sha": "a91957a858320c0e17f3a0eca7cfacbff50ea29a",
                                  "ref": "main",
                                  "status": "success",
                                  "web_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines/47"
                                }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);
        PipelineJobListOptions options = new() { IncludeRetried = true, Scope = ["failed", "running"], PerPage = 30 };

        List<GitLabJob> jobs = [];
        await foreach (GitLabJob job in repository.ListJobsAsync("gitlab-org/gitlab", 47, options,
                           TestContext.Current.CancellationToken))
        {
            jobs.Add(job);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.StartsWith("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/pipelines/47/jobs?",
            requestUri, StringComparison.Ordinal);
        Assert.Contains("include_retried=true", requestUri, StringComparison.Ordinal);
        Assert.Contains("scope[]=failed&scope[]=running", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=30", requestUri, StringComparison.Ordinal);

        GitLabJob only = Assert.Single(jobs);
        Assert.Equal(81, only.Id);
        Assert.Equal("build", only.Name);
        Assert.Equal(47, only.Pipeline?.Id);
    }

    [Fact]
    public async Task ListTriggerJobsAsync_BuildsTheTriggerJobsRoute_AndDeserializesTypedBridge()
    {
        const string Json = """
                            [
                              {
                                "id": 7,
                                "status": "success",
                                "stage": "deploy",
                                "name": "trigger_downstream",
                                "ref": "main",
                                "allow_failure": false,
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/7",
                                "project": {
                                  "ci_job_token_scope_enabled": true
                                },
                                "downstream_pipeline": {
                                  "id": 903,
                                  "project_id": 12,
                                  "sha": "c91957a858320c0e17f3a0eca7cfacbff50ea29a",
                                  "ref": "main",
                                  "status": "running",
                                  "web_url": "https://gitlab.example/gitlab-org/downstream/-/pipelines/903"
                                }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        TriggerJobListOptions options = new() { Scope = ["success", "failed"], Page = 2, PerPage = 20 };

        List<GitLabBridge> bridges = [];
        await foreach (GitLabBridge bridge in repository.ListTriggerJobsAsync(1, 47, options,
                           TestContext.Current.CancellationToken))
        {
            bridges.Add(bridge);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.StartsWith("https://gitlab.example/api/v4/projects/1/pipelines/47/trigger_jobs?", requestUri,
            StringComparison.Ordinal);

        // GitLab expects the repeated form here, exactly as it does on the jobs endpoint.
        Assert.Contains("scope[]=success&scope[]=failed", requestUri, StringComparison.Ordinal);
        Assert.Contains("page=2", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=20", requestUri, StringComparison.Ordinal);

        GitLabBridge only = Assert.Single(bridges);
        Assert.Equal("trigger_downstream", only.Name);
        Assert.True(only.Project?.CiJobTokenScopeEnabled);
        Assert.Equal(903, only.DownstreamPipeline?.Id);
        Assert.Equal("running", only.DownstreamPipeline?.Status);
    }

    [Fact]
    public async Task ListVariablesAsync_BuildsThePipelineVariablesRoute_AndDeserializesVariables()
    {
        const string Json = """
                            [
                              {
                                "key": "RUN_NIGHTLY_BUILD",
                                "variable_type": "env_var",
                                "value": "true"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        List<GitLabVariable> variables = [];
        await foreach (GitLabVariable variable in repository.ListVariablesAsync(1, 47,
                           TestContext.Current.CancellationToken))
        {
            variables.Add(variable);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/1/pipelines/47/variables",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        GitLabVariable only = Assert.Single(variables);
        Assert.Equal("RUN_NIGHTLY_BUILD", only.Key);
        Assert.Equal("env_var", only.VariableType);
        Assert.Equal("true", only.Value);
    }

    [Fact]
    public async Task ListVariablesAsync_OnForbiddenResponse_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        PipelinesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            DrainAsync(repository.ListVariablesAsync(1, 47, TestContext.Current.CancellationToken)));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    /// <summary>
    ///     Enumeration is what issues the request for a paged endpoint, so an error-path assertion has to
    ///     drain the sequence. Kept out of the assertion lambda so the loop lives in an ordinary method body.
    /// </summary>
    private static async Task DrainAsync(IAsyncEnumerable<GitLabVariable> source)
    {
        // ConfigureAwait is otherwise absent from this suite; it is here only because CA2007 flags an
        // "await foreach" over a bare IAsyncEnumerable local, and the analyzer is on for this project.
        await foreach (GitLabVariable _ in source.ConfigureAwait(false))
        {
        }
    }
}