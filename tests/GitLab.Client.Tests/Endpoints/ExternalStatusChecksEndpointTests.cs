using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class ExternalStatusChecksEndpointTests
{
    private const string CheckJson = """
                                     {
                                       "id": 4,
                                       "name": "Compliance gate",
                                       "project_id": 1,
                                       "external_url": "https://checks.example/compliance",
                                       "hmac": true,
                                       "protected_branches": [
                                         {
                                           "id": 3,
                                           "name": "main",
                                           "allow_force_push": false,
                                           "merge_access_levels": [
                                             { "access_level": 40, "access_level_description": "Maintainers" }
                                           ]
                                         }
                                       ]
                                     }
                                     """;

    [Fact]
    public async Task ListAsync_BuildsExternalStatusChecksRoute_AndDeserializesChecks()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, "[" + CheckJson + "]"));
        ExternalStatusChecksClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            List<GitLabExternalStatusCheck> checks = [];
            await foreach (GitLabExternalStatusCheck item in repository.ListAsync(1,
                               TestContext.Current.CancellationToken))
            {
                checks.Add(item);
            }

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/external_status_checks",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            GitLabExternalStatusCheck check = Assert.Single(checks);
            Assert.Equal(4, check.Id);
            Assert.Equal("Compliance gate", check.Name);
            Assert.Equal(1, check.ProjectId);
            Assert.Equal(new Uri("https://checks.example/compliance"), check.ExternalUrl);
            Assert.True(check.Hmac);

            GitLabProtectedBranch branch = Assert.Single(check.ProtectedBranches!);
            Assert.Equal("main", branch.Name);
            Assert.Equal(40, Assert.Single(branch.MergeAccessLevels!).AccessLevel);
        }
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, "[]"));
        ExternalStatusChecksClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await foreach (GitLabExternalStatusCheck _ in repository.ListAsync("gitlab-org/gitlab",
                               TestContext.Current.CancellationToken))
            {
                // Draining the sequence is what issues the request.
            }

            Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/external_status_checks",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task CreateAsync_PostsSerializedBody_AndDeserializesCreatedCheck()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.Created, CheckJson);
        });

        ExternalStatusChecksClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            CreateExternalStatusCheckRequest request = new()
            {
                Name = "Compliance gate",
                ExternalUrl = new Uri("https://checks.example/compliance"),
                SharedSecret = "s3cret",
                ProtectedBranchIds = [3]
            };

            GitLabExternalStatusCheck check =
                await repository.CreateAsync(1, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/external_status_checks",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
            Assert.Contains("\"name\":\"Compliance gate\"", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"external_url\":\"https://checks.example/compliance\"", sentBody,
                StringComparison.Ordinal);
            Assert.Contains("\"shared_secret\":\"s3cret\"", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"protected_branch_ids\":[3]", sentBody, StringComparison.Ordinal);

            // shared_secret is write-only: it never comes back, only the hmac flag does.
            Assert.True(check.Hmac);
            Assert.Equal(4, check.Id);
        }
    }

    [Fact]
    public void RequestRecords_DoNotLeakTheSharedSecretThroughToString()
    {
        CreateExternalStatusCheckRequest create = new()
        {
            Name = "Compliance gate",
            ExternalUrl = new Uri("https://checks.example/compliance"),
            SharedSecret = "s3cret"
        };
        UpdateExternalStatusCheckRequest update = new() { SharedSecret = "s3cret" };

        Assert.DoesNotContain("s3cret", create.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("s3cret", update.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateAsync_PutsSerializedBody_ToTheCheckRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.OK, CheckJson);
        });

        ExternalStatusChecksClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            UpdateExternalStatusCheckRequest request = new()
            {
                ExternalUrl = new Uri("https://checks.example/v2"), ProtectedBranchIds = [3, 9]
            };

            GitLabExternalStatusCheck check =
                await repository.UpdateAsync(1, 4, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/external_status_checks/4",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Contains("\"external_url\":\"https://checks.example/v2\"", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"protected_branch_ids\":[3,9]", sentBody, StringComparison.Ordinal);
            Assert.DoesNotContain("\"name\"", sentBody, StringComparison.Ordinal);
            Assert.DoesNotContain("\"shared_secret\"", sentBody, StringComparison.Ordinal);
            Assert.Equal("Compliance gate", check.Name);
        }
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheCheckRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        ExternalStatusChecksClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.DeleteAsync(1, 4, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/external_status_checks/4",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task DeleteAsync_OnNotFound_ThrowsGitLabNotFoundException()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.NotFound, """{ "message": "404 External Status Check Not Found" }"""));
        ExternalStatusChecksClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
                repository.DeleteAsync(1, 999, TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal("404 External Status Check Not Found", exception.Message);
        }
    }

    [Fact]
    public async Task ListForMergeRequestAsync_ReadsTheStatusChecksRoute_AndDeserializesStatuses()
    {
        const string StatusChecksJson = """
                                        [
                                          {
                                            "id": 4,
                                            "name": "Compliance gate",
                                            "external_url": "https://checks.example/compliance",
                                            "status": "pending"
                                          }
                                        ]
                                        """;

        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, StatusChecksJson));
        ExternalStatusChecksClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            List<GitLabMergeRequestStatusCheck> checks = [];
            await foreach (GitLabMergeRequestStatusCheck item in repository.ListForMergeRequestAsync(1, 12,
                               TestContext.Current.CancellationToken))
            {
                checks.Add(item);
            }

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/12/status_checks",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            GitLabMergeRequestStatusCheck check = Assert.Single(checks);
            Assert.Equal(4, check.Id);
            Assert.Equal("Compliance gate", check.Name);
            Assert.Equal("pending", check.Status);
            Assert.Equal(new Uri("https://checks.example/compliance"), check.ExternalUrl);
        }
    }

    [Fact]
    public async Task SetStatusAsync_PostsToStatusCheckResponses_AndDeserializesTheEmbeddedMergeRequest()
    {
        const string ResponseJson = """
                                    {
                                      "id": 77,
                                      "merge_request": {
                                        "id": 900,
                                        "iid": 12,
                                        "project_id": 1,
                                        "title": "Add compliance gate",
                                        "state": "opened",
                                        "source_branch": "feature/gate",
                                        "target_branch": "main",
                                        "web_url": "https://gitlab.example/gitlab-org/gitlab/-/merge_requests/12"
                                      },
                                      "external_status_check": {
                                        "id": 4,
                                        "name": "Compliance gate",
                                        "external_url": "https://checks.example/compliance"
                                      }
                                    }
                                    """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.Created, ResponseJson);
        });

        ExternalStatusChecksClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            SetStatusCheckStatusRequest request = new()
            {
                ExternalStatusCheckId = 4, Sha = "9a3f2b1c4d5e", Status = "passed"
            };

            GitLabStatusCheckResponse response = await repository.SetStatusAsync("gitlab-org/gitlab", 12, request,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_requests/12/status_check_responses",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Contains("\"external_status_check_id\":4", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"sha\":\"9a3f2b1c4d5e\"", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"status\":\"passed\"", sentBody, StringComparison.Ordinal);

            Assert.Equal(77, response.Id);
            Assert.Equal(12, response.MergeRequest?.Iid);
            Assert.Equal("Add compliance gate", response.MergeRequest?.Title);
            Assert.Equal("opened", response.MergeRequest?.State);
            Assert.Equal("feature/gate", response.MergeRequest?.SourceBranch);
            Assert.Equal("main", response.MergeRequest?.TargetBranch);
            Assert.Equal(4, response.ExternalStatusCheck?.Id);
            Assert.Equal("Compliance gate", response.ExternalStatusCheck?.Name);
            Assert.Equal(new Uri("https://checks.example/compliance"), response.ExternalStatusCheck?.ExternalUrl);
        }
    }

    [Fact]
    public async Task SetStatusAsync_OnConflict_ThrowsGitLabConflictException()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.Conflict, """{ "message": "409 Conflict" }"""));
        ExternalStatusChecksClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            SetStatusCheckStatusRequest request = new() { ExternalStatusCheckId = 4, Sha = "stale", Status = "passed" };

            GitLabApiException exception = await Assert.ThrowsAsync<GitLabConflictException>(() =>
                repository.SetStatusAsync(1, 12, request, TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        }
    }

    [Fact]
    public async Task RetryAsync_PostsToTheRetryRoute_WithNoBody_AndToleratesAnEmpty202()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));
        ExternalStatusChecksClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.RetryAsync(1, 12, 4, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/12/status_checks/4/retry",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Null(handler.LastRequest?.Content);
        }
    }

    [Fact]
    public async Task RetryAsync_WhenTheCheckHasNotFailed_ThrowsGitLabValidationException()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.UnprocessableEntity, """{ "message": "422 Unprocessable Entity" }"""));
        ExternalStatusChecksClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
                repository.RetryAsync(1, 12, 4, TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        }
    }

    private static HttpResponseMessage Json(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private static ExternalStatusChecksClient CreateRepository(StubHttpMessageHandler handler,
        out HttpClient httpClient)
    {
        httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        return new ExternalStatusChecksClient(new GitLabApiConnection(httpClient));
    }
}