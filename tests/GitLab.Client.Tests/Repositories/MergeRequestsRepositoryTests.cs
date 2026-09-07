using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class MergeRequestsRepositoryTests
{
    [Fact]
    public async Task GetAsync_BuildsProjectScopedRoute_AndDeserializesMergeRequest()
    {
        const string Json = """
                            {
                              "id": 501,
                              "iid": 7,
                              "project_id": 42,
                              "title": "Add feature",
                              "state": "opened",
                              "source_branch": "feature/add-thing",
                              "target_branch": "main",
                              "draft": false,
                              "merge_status": "can_be_merged",
                              "web_url": "https://gitlab.example/group/project/-/merge_requests/7"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        GitLabMergeRequest mergeRequest = await repository.GetAsync(42, 7, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(501, mergeRequest.Id);
        Assert.Equal(7, mergeRequest.Iid);
        Assert.Equal("opened", mergeRequest.State);
        Assert.False(mergeRequest.Draft);
        Assert.Equal(
            new Uri("https://gitlab.example/group/project/-/merge_requests/7"),
            mergeRequest.WebUrl);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath()
    {
        const string Json = """
                            {
                              "id": 1,
                              "iid": 1,
                              "title": "x",
                              "state": "opened",
                              "source_branch": "a",
                              "target_branch": "main",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/merge_requests/1"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        await repository.GetAsync("gitlab-org/gitlab", 1, TestContext.Current.CancellationToken);

        Assert.Contains("/projects/gitlab-org%2Fgitlab/merge_requests/1", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListAsync_AppliesStateAndPerPageQuery()
    {
        const string Json = """
                            [
                              { "id": 1, "iid": 1, "title": "First", "state": "opened", "source_branch": "a", "target_branch": "main", "web_url": "https://gitlab.example/group/project/-/merge_requests/1" },
                              { "id": 2, "iid": 2, "title": "Second", "state": "opened", "source_branch": "b", "target_branch": "main", "web_url": "https://gitlab.example/group/project/-/merge_requests/2" }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        List<GitLabMergeRequest> results = new();
        await foreach (GitLabMergeRequest mergeRequest in repository.ListAsync(
                           42,
                           new MergeRequestListOptions { State = MergeRequestStateFilter.Opened, PerPage = 20 },
                           TestContext.Current.CancellationToken))
        {
            results.Add(mergeRequest);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests?state=opened&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, results.Count);
        Assert.Equal("First", results[0].Title);
        Assert.Equal("Second", results[1].Title);
    }

    [Fact]
    public async Task CreateAsync_PostsRequestBody_AndDeserializesCreatedMergeRequest()
    {
        const string Json = """
                            {
                              "id": 900,
                              "iid": 12,
                              "project_id": 42,
                              "title": "New MR",
                              "state": "opened",
                              "source_branch": "feature/x",
                              "target_branch": "main",
                              "web_url": "https://gitlab.example/group/project/-/merge_requests/12"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            // The request Content is disposed by GitLabApiConnection right after the call returns, so
            // capture the body here - while the message is still live - rather than from LastRequest afterward.
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        CreateMergeRequestRequest request = new()
        {
            Title = "New MR", SourceBranch = "feature/x", TargetBranch = "main"
        };

        GitLabMergeRequest mergeRequest =
            await repository.CreateAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Contains("\"title\":\"New MR\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"source_branch\":\"feature/x\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"target_branch\":\"main\"", sentBody, StringComparison.Ordinal);

        Assert.Equal(900, mergeRequest.Id);
        Assert.Equal(12, mergeRequest.Iid);
        Assert.Equal("New MR", mergeRequest.Title);
    }

    [Fact]
    public async Task CreateAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "400 Bad request" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        CreateMergeRequestRequest request = new()
        {
            Title = "New MR", SourceBranch = "feature/x", TargetBranch = "main"
        };

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateAsync(42, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("400 Bad request", exception.Message);
    }

    [Fact]
    public async Task MergeAsync_PutsToTheMergeRoute_SendsTheOptions_AndReturnsTheMergedMergeRequest()
    {
        const string Json = """
                            {
                              "id": 501,
                              "iid": 7,
                              "project_id": 42,
                              "title": "Add feature",
                              "state": "merged",
                              "source_branch": "feature/add-thing",
                              "target_branch": "main",
                              "merge_status": "can_be_merged",
                              "detailed_merge_status": "mergeable",
                              "sha": "a1b2c3",
                              "merge_commit_sha": "d4e5f6",
                              "web_url": "https://gitlab.example/group/project/-/merge_requests/7"
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
        MergeRequestsRepository repository = new(connection);

        GitLabMergeRequest merged = await repository.MergeAsync(
            42,
            7,
            new MergeMergeRequestRequest
            {
                MergeCommitMessage = "Merge it", Squash = true, ShouldRemoveSourceBranch = true, Sha = "a1b2c3"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/merge",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"merge_commit_message\":\"Merge it\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"squash\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"should_remove_source_branch\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"sha\":\"a1b2c3\"", sentBody, StringComparison.Ordinal);
        Assert.Equal("merged", merged.State);
        Assert.Equal("mergeable", merged.DetailedMergeStatus);
        Assert.Equal("d4e5f6", merged.MergeCommitSha);
    }

    [Fact]
    public async Task MergeAsync_WithoutOptions_StillSendsAJsonBody()
    {
        const string Json = """
                            {
                              "id": 1, "iid": 1, "title": "x", "state": "merged",
                              "source_branch": "a", "target_branch": "main",
                              "web_url": "https://gitlab.example/group/project/-/merge_requests/1"
                            }
                            """;

        string? sentBody = null;
        string? sentContentType = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            sentContentType = request.Content?.Headers.ContentType?.MediaType;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        await repository.MergeAsync(42, 1, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("{}", sentBody);
        Assert.Equal("application/json", sentContentType);
    }

    [Fact]
    public async Task MergeAsync_WhenGitLabReportsAConflict_ThrowsGitLabConflictException()
    {
        const string Json = """{ "message": "SHA does not match HEAD of source branch" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        GitLabConflictException exception = await Assert.ThrowsAsync<GitLabConflictException>(() =>
            repository.MergeAsync(42, 7, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        Assert.Equal("SHA does not match HEAD of source branch", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_PutsTheMergeRequest_AndWritesTheStateEventAsItsWireValue()
    {
        const string Json = """
                            {
                              "id": 1, "iid": 7, "title": "Renamed", "state": "closed",
                              "source_branch": "a", "target_branch": "main",
                              "web_url": "https://gitlab.example/group/project/-/merge_requests/7"
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
        MergeRequestsRepository repository = new(connection);

        GitLabMergeRequest updated = await repository.UpdateAsync(
            42,
            7,
            new UpdateMergeRequestRequest
            {
                Title = "Renamed", StateEvent = MergeRequestStateEvent.Close, AddLabels = ["bug", "needs review"]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"state_event\":\"close\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"add_labels\":[\"bug\",\"needs review\"]", sentBody, StringComparison.Ordinal);

        // Everything left null must be omitted, or the PUT would clear it server-side.
        Assert.DoesNotContain("\"description\"", sentBody, StringComparison.Ordinal);
        Assert.Equal("closed", updated.State);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheMergeRequestRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        await repository.DeleteAsync("gitlab-org/gitlab", 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_requests/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RebaseAsync_PutsSkipCi_AndReadsTheAcceptedResult()
    {
        const string Json = """{ "rebase_in_progress": true }""";

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        GitLabMergeRequestRebaseResult result = await repository.RebaseAsync(
            42,
            7,
            new RebaseMergeRequestRequest { SkipCi = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/rebase",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"skip_ci\":true", sentBody, StringComparison.Ordinal);
        Assert.True(result.RebaseInProgress);
    }

    [Fact]
    public async Task CancelMergeWhenPipelineSucceedsAsync_PostsWithNoBody()
    {
        const string Json = """
                            {
                              "id": 1, "iid": 7, "title": "x", "state": "opened",
                              "source_branch": "a", "target_branch": "main",
                              "merge_when_pipeline_succeeds": false,
                              "web_url": "https://gitlab.example/group/project/-/merge_requests/7"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        GitLabMergeRequest mergeRequest = await repository.CancelMergeWhenPipelineSucceedsAsync(
            42, 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/cancel_merge_when_pipeline_succeeds",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.False(mergeRequest.MergeWhenPipelineSucceeds);
    }

    [Fact]
    public async Task GetMergeRefAsync_ReadsTheMergeCommitSha()
    {
        const string Json = """{ "commit_id": "854a3a7a17acbcc0bbbea170986e768382c662d4" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        GitLabMergeRequestMergeRef mergeRef =
            await repository.GetMergeRefAsync(42, 7, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/merge_ref",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("854a3a7a17acbcc0bbbea170986e768382c662d4", mergeRef.CommitId);
    }

    [Fact]
    public async Task GetChangesAsync_AppliesUnidiff_AndDeserializesTheFileDiffs()
    {
        const string Json = """
                            {
                              "id": 501,
                              "iid": 7,
                              "title": "Add feature",
                              "state": "opened",
                              "source_branch": "feature/add-thing",
                              "target_branch": "main",
                              "changes_count": "2",
                              "web_url": "https://gitlab.example/group/project/-/merge_requests/7",
                              "changes": [
                                {
                                  "old_path": "src/old.cs",
                                  "new_path": "src/new.cs",
                                  "a_mode": "100644",
                                  "b_mode": "100755",
                                  "diff": "@@ -1 +1 @@",
                                  "new_file": false,
                                  "renamed_file": true,
                                  "deleted_file": false
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
        MergeRequestsRepository repository = new(connection);

        GitLabMergeRequest changes =
            await repository.GetChangesAsync(42, 7, true, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/changes?unidiff=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("2", changes.ChangesCount);
        GitLabDiff diff = Assert.Single(changes.Changes ?? []);
        Assert.Equal("src/old.cs", diff.OldPath);
        Assert.Equal("src/new.cs", diff.NewPath);
        Assert.Equal("100644", diff.AMode);
        Assert.Equal("100755", diff.BMode);
        Assert.True(diff.RenamedFile);
    }

    [Fact]
    public async Task ListDiffsAsync_AppliesTheDiffOptions_AndStreamsTheEntries()
    {
        const string Json = """
                            [
                              { "old_path": "a.cs", "new_path": "a.cs", "a_mode": "100644", "b_mode": "100644", "diff": "@@", "new_file": false, "renamed_file": false, "deleted_file": false },
                              { "old_path": "b.cs", "new_path": "b.cs", "a_mode": "100644", "b_mode": "100644", "diff": "@@", "new_file": true, "renamed_file": false, "deleted_file": false }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        List<GitLabDiff> diffs = [];
        await foreach (GitLabDiff diff in repository.ListDiffsAsync(
                           42,
                           7,
                           new MergeRequestDiffListOptions { Unidiff = true, PerPage = 50 },
                           TestContext.Current.CancellationToken))
        {
            diffs.Add(diff);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/diffs?unidiff=true&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, diffs.Count);
        Assert.True(diffs[1].NewFile);
    }

    [Fact]
    public async Task GetRawDiffsAsync_StreamsTheRawPatchBody()
    {
        const string Patch = "diff --git a/a.cs b/a.cs";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Patch, Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        GitLabFileResponse response =
            await repository.GetRawDiffsAsync(42, 7, TestContext.Current.CancellationToken);

        Assert.Equal("text/plain", response.ContentType);

        string body;
        using (StreamReader reader = new(response.Content, Encoding.UTF8))
        {
            body = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        }

        await response.DisposeAsync();

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/raw_diffs",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(Patch, body);
    }

    [Fact]
    public async Task ListCommitsAsync_StreamsTheMergeRequestCommits()
    {
        const string Json = """
                            [
                              {
                                "id": "ed899a2f4b50b4370feeea94676502b42383c746",
                                "short_id": "ed899a2f",
                                "title": "Fix the thing",
                                "author_name": "Ada",
                                "web_url": "https://gitlab.example/group/project/-/commit/ed899a2f"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        List<GitLabCommit> commits = [];
        await foreach (GitLabCommit commit in repository.ListCommitsAsync(
                           42, 7, TestContext.Current.CancellationToken))
        {
            commits.Add(commit);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/commits",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("ed899a2f", Assert.Single(commits).ShortId);
    }

    [Fact]
    public async Task AddContextCommitsAsync_PostsTheShas_AndReturnsTheAttachedCommits()
    {
        const string Json = """
                            [
                              { "id": "abc123", "short_id": "abc123", "title": "Context", "web_url": "https://gitlab.example/group/project/-/commit/abc123" }
                            ]
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
        MergeRequestsRepository repository = new(connection);

        IReadOnlyList<GitLabCommit> commits = await repository.AddContextCommitsAsync(
            42,
            7,
            new CreateMergeRequestContextCommitsRequest { Commits = ["abc123"] },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/context_commits",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"commits\":[\"abc123\"]", sentBody, StringComparison.Ordinal);
        Assert.Equal("abc123", Assert.Single(commits).Id);
    }

    [Fact]
    public async Task RemoveContextCommitsAsync_SendsTheShasAsAQueryParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        await repository.RemoveContextCommitsAsync(
            42, 7, ["abc123", "def456"], TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/context_commits?commits=abc123,def456",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SetTimeEstimateAsync_PostsTheDuration_AndReadsTheTotals()
    {
        const string Json = """
                            {
                              "time_estimate": 12600,
                              "total_time_spent": 3600,
                              "human_time_estimate": "3h 30m",
                              "human_total_time_spent": "1h"
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
        MergeRequestsRepository repository = new(connection);

        GitLabTimeStats stats = await repository.SetTimeEstimateAsync(
            42,
            7,
            new MergeRequestTimeEstimateRequest { Duration = "3h30m" },
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/time_estimate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"duration\":\"3h30m\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(12600, stats.TimeEstimate);
        Assert.Equal("1h", stats.HumanTotalTimeSpent);
    }

    [Theory]
    [InlineData("reset_time_estimate")]
    [InlineData("reset_spent_time")]
    public async Task TimeTrackingResets_PostToTheirOwnRoutes(string route)
    {
        const string Json = """{ "time_estimate": 0, "total_time_spent": 0 }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        GitLabTimeStats stats = string.Equals(route, "reset_time_estimate", StringComparison.Ordinal)
            ? await repository.ResetTimeEstimateAsync(42, 7, TestContext.Current.CancellationToken)
            : await repository.ResetSpentTimeAsync(42, 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            $"https://gitlab.example/api/v4/projects/42/merge_requests/7/{route}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(0, stats.TimeEstimate);
    }

    [Fact]
    public async Task AddSpentTimeAsync_PostsTheDurationAndSummary()
    {
        const string Json = """{ "time_estimate": 0, "total_time_spent": 1800 }""";

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
        MergeRequestsRepository repository = new(connection);

        GitLabTimeStats stats = await repository.AddSpentTimeAsync(
            42,
            7,
            new MergeRequestSpentTimeRequest { Duration = "30m", Summary = "Review" },
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/add_spent_time",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"duration\":\"30m\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"summary\":\"Review\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(1800, stats.TotalTimeSpent);
    }

    [Fact]
    public async Task GetTimeStatsAsync_ReadsTheTotals()
    {
        const string Json = """
                            {
                              "time_estimate": 7200,
                              "total_time_spent": 5400,
                              "human_time_estimate": "2h",
                              "human_total_time_spent": "1h 30m"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        GitLabTimeStats stats = await repository.GetTimeStatsAsync(42, 7, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/time_stats",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(7200, stats.TimeEstimate);
        Assert.Equal("1h 30m", stats.HumanTotalTimeSpent);
    }

    [Fact]
    public async Task ListReviewersAsync_DeserializesTheReviewerAndItsState()
    {
        const string Json = """
                            [
                              {
                                "user": {
                                  "id": 9,
                                  "username": "ada",
                                  "name": "Ada Lovelace",
                                  "state": "active",
                                  "web_url": "https://gitlab.example/ada"
                                },
                                "state": "requested_changes",
                                "created_at": "2026-01-02T03:04:05Z"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        List<GitLabMergeRequestReviewer> reviewers = [];
        await foreach (GitLabMergeRequestReviewer reviewer in repository.ListReviewersAsync(
                           42, 7, TestContext.Current.CancellationToken))
        {
            reviewers.Add(reviewer);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/reviewers",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        GitLabMergeRequestReviewer single = Assert.Single(reviewers);
        Assert.Equal("requested_changes", single.State);
        Assert.Equal("ada", single.User?.Username);
    }

    [Fact]
    public async Task GetVersionAsync_AddressesTheVersionById_AndReadsItsCommitsAndDiffs()
    {
        const string Json = """
                            {
                              "id": 110,
                              "head_commit_sha": "33e2ee8579fda5bc36accc9c6fbd0b4fefda9e30",
                              "base_commit_sha": "eeb57dffe83deb686a60a71c16c32f71046868fd",
                              "start_commit_sha": "0b4bc9a49b562e85de7cc9e834518ea6828729b9",
                              "created_at": "2026-01-02T03:04:05Z",
                              "merge_request_id": 105,
                              "state": "collected",
                              "real_size": "1",
                              "commits": [
                                { "id": "33e2ee85", "short_id": "33e2ee85", "title": "Change", "web_url": "https://gitlab.example/group/project/-/commit/33e2ee85" }
                              ],
                              "diffs": [
                                { "old_path": "a.cs", "new_path": "a.cs", "a_mode": "100644", "b_mode": "100644", "diff": "@@", "new_file": false, "renamed_file": false, "deleted_file": false }
                              ]
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        GitLabMergeRequestDiffVersion version =
            await repository.GetVersionAsync(42, 7, 110, true, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/versions/110?unidiff=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(110, version.Id);
        Assert.Equal("collected", version.State);
        Assert.Equal("1", version.RealSize);
        Assert.Single(version.Commits ?? []);
        Assert.Single(version.Diffs ?? []);
    }

    [Fact]
    public async Task ListBlocksAsync_DeserializesBothSidesOfTheDependency()
    {
        const string Json = """
                            [
                              {
                                "id": 1,
                                "project_id": 42,
                                "blocking_merge_request": {
                                  "id": 100, "iid": 3, "title": "Blocker", "state": "opened",
                                  "source_branch": "b", "target_branch": "main",
                                  "web_url": "https://gitlab.example/group/project/-/merge_requests/3"
                                },
                                "blocked_merge_request": {
                                  "id": 101, "iid": 7, "title": "Blocked", "state": "opened",
                                  "source_branch": "a", "target_branch": "main",
                                  "web_url": "https://gitlab.example/group/project/-/merge_requests/7"
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
        MergeRequestsRepository repository = new(connection);

        List<GitLabMergeRequestDependency> blocks = [];
        await foreach (GitLabMergeRequestDependency block in repository.ListBlocksAsync(
                           42, 7, TestContext.Current.CancellationToken))
        {
            blocks.Add(block);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/blocks",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        GitLabMergeRequestDependency single = Assert.Single(blocks);
        Assert.Equal(3, single.BlockingMergeRequest?.Iid);
        Assert.Equal(7, single.BlockedMergeRequest?.Iid);
    }

    [Fact]
    public async Task GetBlockAsync_AddressesOneDependency()
    {
        const string Json = """{ "id": 5, "project_id": 42 }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        GitLabMergeRequestDependency block =
            await repository.GetBlockAsync(42, 7, 5, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/blocks/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(5, block.Id);
    }

    [Fact]
    public async Task CreateBlockAsync_PostsTheBlockingMergeRequest()
    {
        const string Json = """{ "id": 5, "project_id": 42 }""";

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
        MergeRequestsRepository repository = new(connection);

        GitLabMergeRequestDependency block = await repository.CreateBlockAsync(
            42,
            7,
            new CreateMergeRequestDependencyRequest { BlockingMergeRequestId = 100 },
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/blocks",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"blocking_merge_request_id\":100", sentBody, StringComparison.Ordinal);
        Assert.Equal(5, block.Id);
    }

    [Fact]
    public async Task RemoveBlockAsync_AddressesTheBlockByItsOwnId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        await repository.RemoveBlockAsync(42, 7, 5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/blocks/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListAllAsync_BuildsTheInstanceWideRoute_AndProjectsTheRichFilters()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        await foreach (GitLabMergeRequest _ in repository.ListAllAsync(
                           new MergeRequestListOptions
                           {
                               State = MergeRequestStateFilter.Merged,
                               NotLabels = ["wontfix"],
                               Scope = MergeRequestScope.All,
                               OrderBy = MergeRequestOrderBy.MergedAt,
                               Sort = MergeRequestSortDirection.Ascending,
                               MergedAfter = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
                               ApprovedByIds = [7, 9],
                               NonArchived = true
                           },
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub answers with an empty page.");
        }

        string? uri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.StartsWith("https://gitlab.example/api/v4/merge_requests?", uri, StringComparison.Ordinal);
        Assert.Contains("state=merged", uri, StringComparison.Ordinal);
        Assert.Contains("scope=all", uri, StringComparison.Ordinal);
        Assert.Contains("order_by=merged_at", uri, StringComparison.Ordinal);
        Assert.Contains("sort=asc", uri, StringComparison.Ordinal);
        Assert.Contains("merged_after=2026-01-02T03:04:05Z", uri, StringComparison.Ordinal);
        Assert.Contains("approved_by_ids=7,9", uri, StringComparison.Ordinal);
        Assert.Contains("non_archived=true", uri, StringComparison.Ordinal);

        // GitLab spells the negated filters not[...]; the brackets must survive to the wire unescaped.
        Assert.Contains("not[labels]=wontfix", uri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListForGroupAsync_EncodesANamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        await foreach (GitLabMergeRequest _ in repository.ListForGroupAsync(
                           "gitlab-org/subgroup", cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub answers with an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/merge_requests",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Theory]
    [InlineData("closes_issues")]
    [InlineData("related_issues")]
    public async Task IssueListings_HangOffTheMergeRequestRoute(string route)
    {
        const string Json = """
                            [
                              { "id": 1, "iid": 2, "title": "Bug", "state": "opened", "web_url": "https://gitlab.example/group/project/-/issues/2" }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        List<GitLabIssue> issues = [];

        if (string.Equals(route, "closes_issues", StringComparison.Ordinal))
        {
            await foreach (GitLabIssue issue in repository.ListClosesIssuesAsync(
                               42, 7, TestContext.Current.CancellationToken))
            {
                issues.Add(issue);
            }
        }
        else
        {
            await foreach (GitLabIssue issue in repository.ListRelatedIssuesAsync(
                               42, 7, TestContext.Current.CancellationToken))
            {
                issues.Add(issue);
            }
        }

        Assert.Equal(
            $"https://gitlab.example/api/v4/projects/42/merge_requests/7/{route}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, Assert.Single(issues).Iid);
    }

    [Fact]
    public async Task ListParticipantsAsync_StreamsTheUsers()
    {
        const string Json = """
                            [
                              { "id": 9, "username": "ada", "name": "Ada Lovelace", "state": "active", "web_url": "https://gitlab.example/ada" }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        List<GitLabUser> users = [];
        await foreach (GitLabUser user in repository.ListParticipantsAsync(
                           42, 7, TestContext.Current.CancellationToken))
        {
            users.Add(user);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/participants",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("ada", Assert.Single(users).Username);
    }

    [Fact]
    public async Task CreatePipelineAsync_PostsToTheMergeRequestPipelinesRoute()
    {
        const string Json = """
                            {
                              "id": 61,
                              "iid": 3,
                              "project_id": 42,
                              "sha": "a91957a8",
                              "ref": "refs/merge-requests/7/head",
                              "status": "pending",
                              "source": "merge_request_event",
                              "web_url": "https://gitlab.example/group/project/-/pipelines/61"
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
        MergeRequestsRepository repository = new(connection);

        GitLabPipeline pipeline = await repository.CreatePipelineAsync(
            42,
            7,
            new CreateMergeRequestPipelineRequest { Async = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/pipelines",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"async\":true", sentBody, StringComparison.Ordinal);
        Assert.Equal(61, pipeline.Id);
        Assert.Equal("pending", pipeline.Status);
    }

    [Fact]
    public async Task ListPipelinesAsync_StreamsTheMergeRequestPipelines()
    {
        const string Json = """
                            [
                              { "id": 61, "project_id": 42, "sha": "a91957a8", "ref": "refs/merge-requests/7/head", "status": "success", "web_url": "https://gitlab.example/group/project/-/pipelines/61" }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        List<GitLabPipeline> pipelines = [];
        await foreach (GitLabPipeline pipeline in repository.ListPipelinesAsync(
                           42, 7, TestContext.Current.CancellationToken))
        {
            pipelines.Add(pipeline);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/pipelines",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("success", Assert.Single(pipelines).Status);
    }

    [Fact]
    public async Task ListBlockeesAsync_HangsOffTheMergeRequestRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        await foreach (GitLabMergeRequestDependency _ in repository.ListBlockeesAsync(
                           42, 7, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub answers with an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/blockees",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListContextCommitsAsync_HangsOffTheMergeRequestRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        await foreach (GitLabCommit _ in repository.ListContextCommitsAsync(
                           42, 7, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub answers with an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/context_commits",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListVersionsAsync_HangsOffTheMergeRequestRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeRequestsRepository repository = new(connection);

        await foreach (GitLabMergeRequestDiffVersion _ in repository.ListVersionsAsync(
                           42, 7, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub answers with an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/merge_requests/7/versions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}