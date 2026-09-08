using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class DeploymentsRepositoryTests
{
    private const string BaseAddress = "https://gitlab.example/api/v4/";

    [Fact]
    public async Task ListAsync_BuildsDeploymentsRoute_WithQueryOptions_AndDeserializesEachDeployment()
    {
        const string Json = """
                            [
                              {
                                "id": 42,
                                "iid": 2,
                                "ref": "main",
                                "sha": "99d03678b90d914dbb1b109132516d71a4a03ea8",
                                "status": "success",
                                "created_at": "2026-01-01T09:00:00Z",
                                "updated_at": "2026-01-01T09:05:00Z",
                                "user": {
                                  "id": 7,
                                  "username": "release-bot",
                                  "name": "Release Bot",
                                  "state": "active",
                                  "web_url": "https://gitlab.example/release-bot"
                                },
                                "environment": {
                                  "id": 9,
                                  "name": "production",
                                  "slug": "production",
                                  "external_url": "https://shop.example.com",
                                  "created_at": "2025-12-01T00:00:00Z",
                                  "updated_at": "2026-01-01T09:05:00Z"
                                },
                                "deployable": {
                                  "id": 664,
                                  "status": "success",
                                  "stage": "deploy",
                                  "name": "deploy_to_production",
                                  "ref": "main",
                                  "tag": false,
                                  "duration": 12.5,
                                  "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/664"
                                }
                              },
                              {
                                "id": 41,
                                "iid": 1,
                                "ref": "main",
                                "sha": "a91957a858320c0e17f3a0eca7cfacbff50ea29a",
                                "status": "blocked"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeploymentsRepository repository = new(connection);

        DeploymentListOptions options = new()
        {
            OrderBy = "created_at",
            Sort = "desc",
            Environment = "production",
            Status = "success",
            UpdatedAfter = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedBefore = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero),
            FinishedAfter = new DateTimeOffset(2026, 1, 1, 6, 0, 0, TimeSpan.Zero),
            FinishedBefore = new DateTimeOffset(2026, 1, 31, 18, 0, 0, TimeSpan.Zero),
            PerPage = 50
        };

        List<GitLabDeployment> deployments = new();
        await foreach (GitLabDeployment deployment in repository.ListAsync(ProjectId.FromId(5), options,
                           TestContext.Current.CancellationToken))
        {
            deployments.Add(deployment);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/5/deployments?", requestUri, StringComparison.Ordinal);
        Assert.Contains("order_by=created_at", requestUri, StringComparison.Ordinal);
        Assert.Contains("sort=desc", requestUri, StringComparison.Ordinal);
        Assert.Contains("environment=production", requestUri, StringComparison.Ordinal);
        Assert.Contains("status=success", requestUri, StringComparison.Ordinal);
        Assert.Contains("updated_after=2026-01-01T00:00:00Z", requestUri, StringComparison.Ordinal);
        Assert.Contains("updated_before=2026-02-01T00:00:00Z", requestUri, StringComparison.Ordinal);
        Assert.Contains("finished_after=2026-01-01T06:00:00Z", requestUri, StringComparison.Ordinal);
        Assert.Contains("finished_before=2026-01-31T18:00:00Z", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=50", requestUri, StringComparison.Ordinal);

        Assert.Equal(2, deployments.Count);

        GitLabDeployment latest = deployments[0];
        Assert.Equal(42, latest.Id);
        Assert.Equal(2, latest.Iid);
        Assert.Equal("main", latest.Ref);
        Assert.Equal("99d03678b90d914dbb1b109132516d71a4a03ea8", latest.Sha);
        Assert.Equal("success", latest.Status);
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero), latest.CreatedAt);
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 9, 5, 0, TimeSpan.Zero), latest.UpdatedAt);
        Assert.Equal("release-bot", latest.User?.Username);
        Assert.Equal(9, latest.Environment?.Id);
        Assert.Equal("production", latest.Environment?.Name);

        // The nested environment is APIEntitiesEnvironmentBasic, which carries no "state" at all.
        Assert.Null(latest.Environment?.State);
        Assert.Equal(664, latest.Deployable?.Id);
        Assert.Equal("deploy_to_production", latest.Deployable?.Name);
        Assert.Equal("deploy", latest.Deployable?.Stage);

        // pending_approval_count and approvals are extended-shape only, so the list endpoint leaves them null.
        Assert.Null(latest.PendingApprovalCount);
        Assert.Null(latest.Approvals);

        Assert.Equal(41, deployments[1].Id);
        Assert.Equal("blocked", deployments[1].Status);
        Assert.Null(deployments[1].Environment);
        Assert.Null(deployments[1].Deployable);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeploymentsRepository repository = new(connection);

        await foreach (GitLabDeployment _ in repository.ListAsync(ProjectId.FromPath("gitlab-org/gitlab"),
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/deployments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_BuildsSingleDeploymentRoute_AndDeserializesTheExtendedShape()
    {
        const string Json = """
                            {
                              "id": 42,
                              "iid": 2,
                              "ref": "main",
                              "sha": "99d03678b90d914dbb1b109132516d71a4a03ea8",
                              "status": "blocked",
                              "created_at": "2026-01-01T09:00:00Z",
                              "pending_approval_count": 2,
                              "environment": {
                                "id": 9,
                                "name": "production",
                                "slug": "production"
                              },
                              "approvals": {
                                "user": {
                                  "id": 100,
                                  "username": "security-lead",
                                  "name": "Security Lead",
                                  "state": "active",
                                  "web_url": "https://gitlab.example/security-lead"
                                },
                                "status": "approved",
                                "created_at": "2026-01-01T09:30:00Z",
                                "comment": "Looks safe."
                              }
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeploymentsRepository repository = new(connection);

        GitLabDeployment deployment = await repository.GetAsync(5, 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/deployments/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(42, deployment.Id);
        Assert.Equal("blocked", deployment.Status);
        Assert.Equal(2, deployment.PendingApprovalCount);
        Assert.Equal("production", deployment.Environment?.Name);
        Assert.Null(deployment.Deployable);

        // approvals is extended-shape only, like pending_approval_count.
        Assert.Equal("approved", deployment.Approvals?.Status);
        Assert.Equal("security-lead", deployment.Approvals?.User?.Username);
        Assert.Equal("Looks safe.", deployment.Approvals?.Comment);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath()
    {
        const string Json = """{ "id": 42, "status": "success" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeploymentsRepository repository = new(connection);

        await repository.GetAsync("gitlab-org/gitlab", 42, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/deployments/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_PostsToDeploymentsRoute_WithSerializedBody_AndDeserializesTheCreatedDeployment()
    {
        const string Json = """
                            {
                              "id": 43,
                              "iid": 3,
                              "ref": "v1.2.0",
                              "sha": "a91957a858320c0e17f3a0eca7cfacbff50ea29a",
                              "status": "success",
                              "pending_approval_count": 0
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

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeploymentsRepository repository = new(connection);

        CreateDeploymentRequest request = new()
        {
            Environment = "production",
            Sha = "a91957a858320c0e17f3a0eca7cfacbff50ea29a",
            Ref = "v1.2.0",
            Tag = true,
            Status = "success"
        };

        GitLabDeployment deployment = await repository.CreateAsync(5, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/deployments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"environment\":\"production\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"sha\":\"a91957a858320c0e17f3a0eca7cfacbff50ea29a\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"ref\":\"v1.2.0\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"tag\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"status\":\"success\"", sentBody, StringComparison.Ordinal);

        Assert.Equal(43, deployment.Id);
        Assert.Equal("v1.2.0", deployment.Ref);
        Assert.Equal(0, deployment.PendingApprovalCount);
    }

    [Fact]
    public async Task UpdateAsync_PutsToSingleDeploymentRoute_WithSerializedBody()
    {
        const string Json = """{ "id": 42, "status": "failed" }""";

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
        DeploymentsRepository repository = new(connection);

        UpdateDeploymentRequest request = new() { Status = "failed" };

        GitLabDeployment deployment =
            await repository.UpdateAsync(5, 42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/deployments/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"status\":\"failed\"}", sentBody);
        Assert.Equal("failed", deployment.Status);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToSingleDeploymentRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeploymentsRepository repository = new(connection);

        await repository.DeleteAsync("gitlab-org/gitlab", 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/deployments/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ApproveAsync_PostsToApprovalRoute_WithSerializedBody_AndDeserializesTheApproval()
    {
        const string Json = """
                            {
                              "user": {
                                "id": 100,
                                "username": "security-lead",
                                "name": "Security Lead",
                                "state": "active",
                                "web_url": "https://gitlab.example/security-lead"
                              },
                              "status": "approved",
                              "created_at": "2026-01-01T10:00:00Z",
                              "comment": "Change ticket CHG-1234 signed off."
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

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeploymentsRepository repository = new(connection);

        ApproveDeploymentRequest request = new()
        {
            Status = "approved", Comment = "Change ticket CHG-1234 signed off.", RepresentedAs = "security"
        };

        GitLabDeploymentApproval approval =
            await repository.ApproveAsync(5, 42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/deployments/42/approval",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"status\":\"approved\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"comment\":\"Change ticket CHG-1234 signed off.\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"represented_as\":\"security\"", sentBody, StringComparison.Ordinal);

        Assert.Equal("approved", approval.Status);
        Assert.Equal("security-lead", approval.User?.Username);
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero), approval.CreatedAt);
        Assert.Equal("Change ticket CHG-1234 signed off.", approval.Comment);
    }

    [Fact]
    public async Task ApproveAsync_OmitsUnsetOptionalFields()
    {
        const string Json = """{ "status": "rejected" }""";

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeploymentsRepository repository = new(connection);

        GitLabDeploymentApproval approval = await repository.ApproveAsync(5, 42,
            new ApproveDeploymentRequest { Status = "rejected" }, TestContext.Current.CancellationToken);

        Assert.Equal("{\"status\":\"rejected\"}", sentBody);
        Assert.Equal("rejected", approval.Status);
        Assert.Null(approval.User);
        Assert.Null(approval.Comment);
    }

    [Fact]
    public async Task ListMergeRequestsAsync_BuildsDeploymentMergeRequestsRoute_WithQueryOptions_AndDeserializes()
    {
        const string Json = """
                            [
                              {
                                "id": 90,
                                "iid": 12,
                                "project_id": 5,
                                "title": "Ship the checkout redesign",
                                "state": "merged",
                                "source_branch": "checkout-redesign",
                                "target_branch": "main",
                                "web_url": "https://gitlab.example/acme/shop/-/merge_requests/12"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeploymentsRepository repository = new(connection);

        MergeRequestListOptions options = new() { State = MergeRequestStateFilter.Merged, PerPage = 20 };

        List<GitLabMergeRequest> mergeRequests = new();
        await foreach (GitLabMergeRequest item in repository.ListMergeRequestsAsync(ProjectId.FromId(5), 42,
                           options, TestContext.Current.CancellationToken))
        {
            mergeRequests.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/5/deployments/42/merge_requests?", requestUri, StringComparison.Ordinal);
        Assert.Contains("state=merged", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=20", requestUri, StringComparison.Ordinal);

        GitLabMergeRequest mergeRequest = Assert.Single(mergeRequests);
        Assert.Equal(90, mergeRequest.Id);
        Assert.Equal(12, mergeRequest.Iid);
        Assert.Equal("merged", mergeRequest.State);
        Assert.Equal("checkout-redesign", mergeRequest.SourceBranch);
    }

    [Fact]
    public async Task ListMergeRequestsAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeploymentsRepository repository = new(connection);

        await foreach (GitLabMergeRequest _ in repository.ListMergeRequestsAsync(
                           ProjectId.FromPath("gitlab-org/gitlab"), 42,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/deployments/42/merge_requests",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnNotFoundResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Deployment Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeploymentsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(5, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Deployment Not Found", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_OnForbiddenResponse_ThrowsGitLabForbiddenException()
    {
        // GitLab refuses to delete the current deployment for an environment.
        const string Json = """{ "message": "403 Forbidden - Cannot destroy running deployment" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri(BaseAddress) };
        GitLabApiConnection connection = new(httpClient);
        DeploymentsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.DeleteAsync(5, 42, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden - Cannot destroy running deployment", exception.Message);
    }
}