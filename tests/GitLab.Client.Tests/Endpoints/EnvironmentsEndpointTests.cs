using System.Globalization;
using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class EnvironmentsEndpointTests
{
    [Fact]
    public async Task ListAsync_BuildsEnvironmentsRoute_AndDeserializesEachEnvironment()
    {
        const string Json = """
                            [
                              {
                                "id": 1,
                                "name": "review/feature",
                                "slug": "review-feature",
                                "external_url": "https://review-feature.example.com",
                                "state": "available",
                                "tier": "development",
                                "auto_stop_at": "2019-05-30T18:55:13.252Z",
                                "auto_stop_setting": "always",
                                "description": "Review app for the feature branch",
                                "kubernetes_namespace": "review-feature",
                                "flux_resource_path": "helm.toolkit.fluxcd.io/v2/namespaces/gitlab/helmreleases/app",
                                "project": {
                                  "id": 42,
                                  "name": "gitlab",
                                  "path_with_namespace": "gitlab-org/gitlab",
                                  "web_url": "https://gitlab.example/gitlab-org/gitlab"
                                },
                                "last_deployment": {
                                  "id": 41,
                                  "iid": 1,
                                  "ref": "main",
                                  "sha": "99d03678b90d914dbb1b109132516d71a4a03ea8",
                                  "created_at": "2016-08-11T11:32:35.444Z",
                                  "updated_at": "2016-08-11T11:33:35.444Z",
                                  "status": "success",
                                  "user": {
                                    "id": 3,
                                    "username": "ada",
                                    "name": "Ada Lovelace",
                                    "web_url": "https://gitlab.example/ada"
                                  },
                                  "environment": {
                                    "id": 1,
                                    "name": "review/feature",
                                    "slug": "review-feature",
                                    "external_url": "https://review-feature.example.com",
                                    "created_at": "2019-05-25T18:55:13.252Z",
                                    "updated_at": "2019-05-27T18:55:13.252Z"
                                  },
                                  "deployable": {
                                    "id": 100,
                                    "status": "success",
                                    "name": "deploy",
                                    "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/100"
                                  }
                                },
                                "cluster_agent": {
                                  "id": 9,
                                  "name": "review-agent",
                                  "config_project": {
                                    "id": 42,
                                    "name": "gitlab",
                                    "path_with_namespace": "gitlab-org/gitlab"
                                  },
                                  "created_at": "2024-01-02T03:04:05Z",
                                  "created_by_user_id": 3,
                                  "is_receptive": true
                                },
                                "created_at": "2019-05-25T18:55:13.252Z",
                                "updated_at": "2019-05-27T18:55:13.252Z"
                              },
                              {
                                "id": 2,
                                "name": "production",
                                "slug": "production",
                                "state": "stopped"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EnvironmentsClient repository = new(connection);

        List<GitLabEnvironment> environments = new();
        await foreach (GitLabEnvironment environment in
                       repository.ListAsync(42, cancellationToken: TestContext.Current.CancellationToken))
        {
            environments.Add(environment);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/environments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, environments.Count);
        Assert.Equal("review/feature", environments[0].Name);
        Assert.Equal("available", environments[0].State);
        Assert.Equal(new Uri("https://review-feature.example.com"), environments[0].ExternalUrl);
        Assert.Equal("development", environments[0].Tier);
        Assert.Equal(DateTimeOffset.Parse("2019-05-30T18:55:13.252Z", CultureInfo.InvariantCulture),
            environments[0].AutoStopAt);
        Assert.Equal("always", environments[0].AutoStopSetting);
        Assert.Equal("Review app for the feature branch", environments[0].Description);
        Assert.Equal("review-feature", environments[0].KubernetesNamespace);
        Assert.Equal("helm.toolkit.fluxcd.io/v2/namespaces/gitlab/helmreleases/app",
            environments[0].FluxResourcePath);
        Assert.Equal(42, environments[0].Project?.Id);
        Assert.Equal("gitlab", environments[0].Project?.Name);
        Assert.Equal("gitlab-org/gitlab", environments[0].Project?.PathWithNamespace);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/gitlab"), environments[0].Project?.WebUrl);
        Assert.Equal(41, environments[0].LastDeployment?.Id);
        Assert.Equal(1, environments[0].LastDeployment?.Iid);
        Assert.Equal("main", environments[0].LastDeployment?.Ref);
        Assert.Equal("99d03678b90d914dbb1b109132516d71a4a03ea8", environments[0].LastDeployment?.Sha);
        Assert.Equal("success", environments[0].LastDeployment?.Status);
        Assert.Equal("ada", environments[0].LastDeployment?.User?.Username);
        Assert.Equal("review/feature", environments[0].LastDeployment?.Environment?.Name);
        Assert.Equal("deploy", environments[0].LastDeployment?.Deployable?.Name);
        Assert.Equal(9, environments[0].ClusterAgent?.Id);
        Assert.Equal("review-agent", environments[0].ClusterAgent?.Name);
        Assert.Equal(42, environments[0].ClusterAgent?.ConfigProject?.Id);
        Assert.Equal("gitlab", environments[0].ClusterAgent?.ConfigProject?.Name);
        Assert.Equal("gitlab-org/gitlab", environments[0].ClusterAgent?.ConfigProject?.PathWithNamespace);
        Assert.Equal(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero), environments[0].ClusterAgent?.CreatedAt);
        Assert.Equal(3, environments[0].ClusterAgent?.CreatedByUserId);
        Assert.True(environments[0].ClusterAgent?.IsReceptive);
        Assert.Equal("production", environments[1].Name);
        Assert.Equal("stopped", environments[1].State);
        Assert.Null(environments[1].ExternalUrl);
        Assert.Null(environments[1].Tier);
        Assert.Null(environments[1].AutoStopAt);
    }

    [Fact]
    public async Task ListAsync_ProjectsTheListOptionsOntoTheQueryString()
    {
        const string Json = "[]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EnvironmentsClient repository = new(connection);

        EnvironmentListOptions options = new()
        {
            Name = "review/feature",
            Search = "review",
            States = "available",
            Page = 2,
            PerPage = 20
        };

        await foreach (GitLabEnvironment _ in repository.ListAsync("gitlab-org/gitlab", options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("/projects/gitlab-org%2Fgitlab/environments?", requestUri, StringComparison.Ordinal);
        Assert.Contains("name=review%2Ffeature", requestUri, StringComparison.Ordinal);
        Assert.Contains("search=review", requestUri, StringComparison.Ordinal);
        Assert.Contains("states=available", requestUri, StringComparison.Ordinal);
        Assert.Contains("page=2", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=20", requestUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath_AndDeserializesEnvironment()
    {
        const string Json = """
                            {
                              "id": 7,
                              "name": "staging",
                              "slug": "staging",
                              "external_url": "https://staging.example.com",
                              "state": "available",
                              "tier": "staging",
                              "created_at": "2019-05-25T18:55:13.252Z",
                              "updated_at": "2019-05-27T18:55:13.252Z"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EnvironmentsClient repository = new(connection);

        GitLabEnvironment environment =
            await repository.GetAsync("gitlab-org/gitlab", 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/environments/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(7, environment.Id);
        Assert.Equal("staging", environment.Name);
        Assert.Equal("staging", environment.Slug);
        Assert.Equal("staging", environment.Tier);
        Assert.Equal("available", environment.State);
        Assert.Equal(new Uri("https://staging.example.com"), environment.ExternalUrl);
        Assert.Equal(DateTimeOffset.Parse("2019-05-25T18:55:13.252Z", CultureInfo.InvariantCulture),
            environment.CreatedAt);
        Assert.Equal(DateTimeOffset.Parse("2019-05-27T18:55:13.252Z", CultureInfo.InvariantCulture),
            environment.UpdatedAt);
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
        EnvironmentsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(42, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_PostsToEnvironmentsRoute_WithSerializedBody_AndDeserializesCreatedEnvironment()
    {
        const string Json = """
                            {
                              "id": 15,
                              "name": "review/new-feature",
                              "slug": "review-new-feature",
                              "external_url": "https://review-new-feature.example.com",
                              "state": "available",
                              "tier": "development",
                              "auto_stop_setting": "with_action"
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
        EnvironmentsClient repository = new(connection);

        CreateEnvironmentRequest request = new()
        {
            Name = "review/new-feature",
            ExternalUrl = new Uri("https://review-new-feature.example.com"),
            Tier = "development",
            ClusterAgentId = 3,
            KubernetesNamespace = "review-new-feature",
            FluxResourcePath = "helm.toolkit.fluxcd.io/v2/namespaces/gitlab/helmreleases/app",
            Description = "Review app",
            AutoStopSetting = "with_action"
        };

        GitLabEnvironment environment =
            await repository.CreateAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/environments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"name\":\"review/new-feature\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"external_url\":\"https://review-new-feature.example.com", sentBody,
            StringComparison.Ordinal);
        Assert.Contains("\"tier\":\"development\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"cluster_agent_id\":3", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"kubernetes_namespace\":\"review-new-feature\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"auto_stop_setting\":\"with_action\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(15, environment.Id);
        Assert.Equal("review/new-feature", environment.Name);
        Assert.Equal("available", environment.State);
        Assert.Equal("development", environment.Tier);
        Assert.Equal("with_action", environment.AutoStopSetting);
    }

    [Fact]
    public async Task UpdateAsync_PutsToEnvironmentRoute_WithOnlyTheSpecifiedFields()
    {
        const string Json = """
                            {
                              "id": 7,
                              "name": "staging",
                              "slug": "staging",
                              "external_url": "https://staging.example.com",
                              "state": "available",
                              "tier": "staging",
                              "description": "Pre-production"
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
        EnvironmentsClient repository = new(connection);

        UpdateEnvironmentRequest request = new() { Tier = "staging", Description = "Pre-production" };

        GitLabEnvironment environment =
            await repository.UpdateAsync("gitlab-org/gitlab", 7, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/environments/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // Unset members are omitted rather than sent as null, so a partial update cannot clear external_url.
        Assert.Equal("""{"tier":"staging","description":"Pre-production"}""", sentBody);

        Assert.Equal("staging", environment.Tier);
        Assert.Equal("Pre-production", environment.Description);
    }

    [Fact]
    public async Task StopAsync_PostsToStopRoute_WithNoBody_AndDeserializesUpdatedEnvironment()
    {
        const string Json = """
                            {
                              "id": 7,
                              "name": "staging",
                              "slug": "staging",
                              "state": "stopped"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EnvironmentsClient repository = new(connection);

        GitLabEnvironment environment = await repository.StopAsync(42, 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/environments/7/stop",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal("stopped", environment.State);
    }

    [Fact]
    public async Task StopAsync_WithForce_PostsTheForceFlagAsTheRequestBody()
    {
        const string Json = """
                            {
                              "id": 7,
                              "name": "staging",
                              "slug": "staging",
                              "state": "stopped"
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
        EnvironmentsClient repository = new(connection);

        GitLabEnvironment environment =
            await repository.StopAsync("gitlab-org/gitlab", 7, true, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/environments/7/stop",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal("""{"force":true}""", sentBody);
        Assert.Equal("stopped", environment.State);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToEnvironmentRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EnvironmentsClient repository = new(connection);

        await repository.DeleteAsync(42, 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/environments/7",
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
        EnvironmentsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.DeleteAsync(42, 7, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task StopStaleAsync_PostsTheCutOffDateToTheStopStaleRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EnvironmentsClient repository = new(connection);

        StopStaleEnvironmentsRequest request = new()
        {
            Before = new DateTimeOffset(2024, 3, 1, 12, 0, 0, TimeSpan.Zero)
        };

        await repository.StopStaleAsync("gitlab-org/gitlab", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/environments/stop_stale",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("2024-03-01T12:00:00", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteReviewAppsAsync_SendsDeleteToTheReviewAppsRoute_WithTheOptionsAsQueryParameters()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {
                  "scheduled_entries": [
                    {
                      "id": 387,
                      "name": "review/023f1bce01229c686a73",
                      "slug": "review-023f1bce01-3uxznk",
                      "external_url": null
                    }
                  ],
                  "unprocessable_entries": [
                    {
                      "id": 388,
                      "name": "review/current",
                      "slug": "review-current",
                      "external_url": "https://review-current.example.com"
                    }
                  ]
                }
                """, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EnvironmentsClient repository = new(connection);

        ReviewAppDeletionOptions options = new()
        {
            Before = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero), Limit = 10, DryRun = false
        };

        GitLabReviewAppDeletionResult result =
            await repository.DeleteReviewAppsAsync("gitlab-org/gitlab", options, TestContext.Current.CancellationToken);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Contains("/projects/gitlab-org%2Fgitlab/environments/review_apps?", requestUri,
            StringComparison.Ordinal);
        Assert.Contains("before=2024-02-01T00:00:00Z", requestUri, StringComparison.Ordinal);
        Assert.Contains("limit=10", requestUri, StringComparison.Ordinal);
        Assert.Contains("dry_run=false", requestUri, StringComparison.Ordinal);
        GitLabEnvironment scheduled = Assert.Single(result.ScheduledEntries!);
        Assert.Equal(387, scheduled.Id);
        Assert.Equal("review/023f1bce01229c686a73", scheduled.Name);
        Assert.Null(scheduled.ExternalUrl);
        GitLabEnvironment unprocessable = Assert.Single(result.UnprocessableEntries!);
        Assert.Equal(388, unprocessable.Id);
        Assert.Equal(new Uri("https://review-current.example.com"), unprocessable.ExternalUrl);
    }

    [Fact]
    public async Task DeleteReviewAppsAsync_WithoutOptions_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{ "scheduled_entries": [] }""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        EnvironmentsClient repository = new(connection);

        GitLabReviewAppDeletionResult result =
            await repository.DeleteReviewAppsAsync(42, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/environments/review_apps",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Empty(result.ScheduledEntries!);
    }
}