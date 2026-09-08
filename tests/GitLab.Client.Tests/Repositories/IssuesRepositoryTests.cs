using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class IssuesRepositoryTests
{
    private const string BasicUserJson = """
                                         {
                                           "id": 9,
                                           "username": "root",
                                           "name": "Administrator",
                                           "state": "active",
                                           "web_url": "https://gitlab.example/root"
                                         }
                                         """;

    private const string TimeStatsJson = """
                                         {
                                           "time_estimate": 3600,
                                           "total_time_spent": 1800,
                                           "human_time_estimate": "1h",
                                           "human_total_time_spent": "30m"
                                         }
                                         """;

    private const string MetricImageJson = """
                                           {
                                             "id": 23,
                                             "created_at": "2024-05-06T07:08:09.000Z",
                                             "filename": "cpu.png",
                                             "file_path": "/uploads/-/system/issuable_metric_image/file/23/cpu.png",
                                             "url": "https://grafana.example/d/abc",
                                             "url_text": "CPU saturation"
                                           }
                                           """;

    private const string MinimalIssueJson = """
                                            {
                                              "id": 76,
                                              "iid": 6,
                                              "title": "Add a README",
                                              "state": "opened",
                                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/issues/6"
                                            }
                                            """;

    private static readonly string[] BugLabel = ["bug"];

    private static readonly string[] ExcludedLabels = ["wontfix"];

    private static StubHttpMessageHandler Responds(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new StubHttpMessageHandler(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }

    [Fact]
    public async Task GetAsync_BuildsIssueRoute_AndDeserializesIssue()
    {
        using StubHttpMessageHandler handler = Responds(MinimalIssueJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        GitLabIssue issue = await repository.GetAsync("gitlab-org/gitlab", 6, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/gitlab-org%2Fgitlab/issues/6", handler.LastRequest?.RequestUri?.AbsoluteUri,
            StringComparison.Ordinal);
        Assert.Equal(76, issue.Id);
        Assert.Equal(6, issue.Iid);
        Assert.Equal("opened", issue.State);
        Assert.Equal("Add a README", issue.Title);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Issue Not Found" }""";

        using StubHttpMessageHandler handler = Responds(Json, HttpStatusCode.NotFound);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync("gitlab-org/gitlab", 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Issue Not Found", exception.Message);
    }

    [Fact]
    public async Task GetAsync_DeserializesTheFullIssuePayload_IncludingItsNestedEntities()
    {
        string json = $$"""
                        {
                          "id": 76,
                          "iid": 6,
                          "project_id": 278964,
                          "title": "Add a README",
                          "description": "The project has none.",
                          "state": "closed",
                          "created_at": "2024-01-02T03:04:05.000Z",
                          "updated_at": "2024-02-03T04:05:06.000Z",
                          "closed_at": "2024-03-04T05:06:07.000Z",
                          "closed_by": {{BasicUserJson}},
                          "author": {{BasicUserJson}},
                          "assignee": {{BasicUserJson}},
                          "assignees": [{{BasicUserJson}}],
                          "labels": ["bug", "needs review"],
                          "milestone": {
                            "id": 3,
                            "iid": 1,
                            "project_id": 278964,
                            "title": "v1.0",
                            "state": "active",
                            "web_url": "https://gitlab.example/gitlab-org/gitlab/-/milestones/1"
                          },
                          "type": "ISSUE",
                          "issue_type": "issue",
                          "user_notes_count": 4,
                          "merge_requests_count": 2,
                          "upvotes": 3,
                          "downvotes": 0,
                          "start_date": "2024-01-01",
                          "due_date": "2024-06-30",
                          "confidential": false,
                          "discussion_locked": null,
                          "web_url": "https://gitlab.example/gitlab-org/gitlab/-/issues/6",
                          "time_stats": {{TimeStatsJson}},
                          "task_completion_status": { "count": 5, "completed_count": 2 },
                          "references": { "short": "#6", "relative": "#6", "full": "gitlab-org/gitlab#6" },
                          "weight": 3,
                          "blocking_issues_count": 1,
                          "has_tasks": true,
                          "task_status": "2 of 5 tasks completed",
                          "severity": "UNKNOWN",
                          "moved_to_id": null,
                          "imported": false,
                          "imported_from": "none",
                          "service_desk_reply_to": null,
                          "health_status": "on_track"
                        }
                        """;

        using StubHttpMessageHandler handler = Responds(json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        GitLabIssue issue = await repository.GetAsync(278964, 6, TestContext.Current.CancellationToken);

        Assert.Equal("closed", issue.State);
        Assert.Equal(new DateTimeOffset(2024, 3, 4, 5, 6, 7, TimeSpan.Zero), issue.ClosedAt);
        Assert.Equal("root", issue.ClosedBy?.Username);
        Assert.Equal("v1.0", issue.Milestone?.Title);
        Assert.Equal(9, Assert.Single(issue.Assignees!).Id);
        Assert.Equal(new DateOnly(2024, 1, 1), issue.StartDate);
        Assert.Equal(new DateOnly(2024, 6, 30), issue.DueDate);
        Assert.Equal(3600, issue.TimeStats?.TimeEstimate);
        Assert.Equal("30m", issue.TimeStats?.HumanTotalTimeSpent);
        Assert.Equal(2, issue.TaskCompletionStatus?.CompletedCount);
        Assert.Equal("gitlab-org/gitlab#6", issue.References?.Full);
        Assert.Equal("#6", issue.References?.ShortReference);
        Assert.Equal("ISSUE", issue.Type);
        Assert.Equal("issue", issue.IssueType);
        Assert.Equal("on_track", issue.HealthStatus);
        Assert.Equal(3, issue.Weight);
        Assert.True(issue.HasTasks);
        Assert.Null(issue.DiscussionLocked);
    }

    [Fact]
    public async Task ListAsync_ProjectsEveryFilter_OntoTheQueryString_InSpecOrder()
    {
        using StubHttpMessageHandler handler = Responds($"[{MinimalIssueJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        IssueListOptions options = new()
        {
            State = GitLabIssueStateFilter.Opened,
            Labels = BugLabel,
            PerPage = 20,
            OrderBy = GitLabIssueOrderBy.UpdatedAt,
            Sort = GitLabIssueSort.Asc,
            DueDate = GitLabIssueDueDateFilter.NoDueDate,
            MilestoneId = GitLabIssueMilestoneFilter.Upcoming,
            AssigneeId = "None",
            Scope = GitLabIssueScope.All,
            EpicId = 12,
            HealthStatus = GitLabIssueHealthStatus.AtRisk,
            NonArchived = true
        };

        List<GitLabIssue> issues = [];
        await foreach (GitLabIssue issue in
                       repository.ListAsync("gitlab-org/gitlab", options, TestContext.Current.CancellationToken))
        {
            issues.Add(issue);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues"
            + "?state=opened&labels=bug&per_page=20&order_by=updated_at&sort=asc&due_date=0"
            + "&milestone_id=Upcoming&assignee_id=None&scope=all&epic_id=12&health_status=at_risk&non_archived=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(76, Assert.Single(issues).Id);
    }

    [Fact]
    public async Task ListForCurrentUserAsync_BuildsTheInstanceWideRoute()
    {
        using StubHttpMessageHandler handler = Responds($"[{MinimalIssueJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        List<GitLabIssue> issues = [];
        await foreach (GitLabIssue issue in repository.ListForCurrentUserAsync(
                           new IssueListOptions { NotLabels = ExcludedLabels, Scope = GitLabIssueScope.AssignedToMe },
                           TestContext.Current.CancellationToken))
        {
            issues.Add(issue);
        }

        string uri = handler.LastRequest?.RequestUri?.AbsoluteUri ?? string.Empty;
        Assert.StartsWith("https://gitlab.example/api/v4/issues?", uri, StringComparison.Ordinal);
        Assert.Contains("not[labels]=wontfix", uri, StringComparison.Ordinal);
        Assert.Contains("scope=assigned_to_me", uri, StringComparison.Ordinal);
        Assert.Single(issues);
    }

    [Fact]
    public async Task GetByIdAsync_BuildsTheInstanceWideIssueRoute()
    {
        using StubHttpMessageHandler handler = Responds(MinimalIssueJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        GitLabIssue issue = await repository.GetByIdAsync(76, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/issues/76", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(76, issue.Id);
    }

    [Fact]
    public async Task CreateAsync_PostsToIssuesRoute_WithSerializedBody_AndDeserializesCreatedIssue()
    {
        const string Json = """
                            {
                              "id": 91,
                              "iid": 12,
                              "title": "New issue",
                              "state": "opened",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/issues/12"
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
        IssuesRepository repository = new(connection);

        CreateIssueRequest request = new()
        {
            Title = "New issue",
            Labels = BugLabel,
            DueDate = new DateOnly(2024, 6, 30),
            IssueType = GitLabIssueType.Incident,
            Severity = GitLabIssueSeverity.High
        };

        GitLabIssue issue = await repository.CreateAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"title\":\"New issue\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"labels\":[\"bug\"]", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"due_date\":\"2024-06-30\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"issue_type\":\"incident\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"severity\":\"high\"", sentBody, StringComparison.Ordinal);

        // Unset members must be omitted rather than sent as null - a null would clear the field.
        Assert.DoesNotContain("\"description\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(91, issue.Id);
    }

    [Fact]
    public async Task UpdateAsync_PutsToTheIssueRoute_WithOnlyTheSuppliedFields()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(MinimalIssueJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        GitLabIssue issue = await repository.UpdateAsync(
            "gitlab-org/gitlab",
            6,
            new UpdateIssueRequest { Title = "Renamed", AddLabels = BugLabel, Confidential = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues/6",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"title\":\"Renamed\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"add_labels\":[\"bug\"]", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"confidential\":true", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("state_event", sentBody, StringComparison.Ordinal);
        Assert.Equal(76, issue.Id);
    }

    [Theory]
    [InlineData(true, "close")]
    [InlineData(false, "reopen")]
    public async Task CloseAndReopen_SendTheMatchingStateEvent(bool close, string expectedEvent)
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(MinimalIssueJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        _ = close
            ? await repository.CloseAsync(42, 6, TestContext.Current.CancellationToken)
            : await repository.ReopenAsync(42, 6, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal($"{{\"state_event\":\"{expectedEvent}\"}}", sentBody);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheIssueRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.DeleteAsync("gitlab-org/gitlab", 6, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues/6",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task MoveAsync_PostsToTheMoveRoute_WithTheTargetProject()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(MinimalIssueJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.MoveAsync(42, 6, new MoveIssueRequest { ToProjectId = 7 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/move",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"to_project_id\":7}", sentBody);
    }

    [Fact]
    public async Task CloneAsync_PostsToTheCloneRoute_WithTheNotesFlag()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(MinimalIssueJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.CloneAsync(42, 6, new CloneIssueRequest { ToProjectId = 7, WithNotes = false },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/clone",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"to_project_id\":7", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"with_notes\":false", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReorderAsync_PutsToTheReorderRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(MinimalIssueJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.ReorderAsync(42, 6, new ReorderIssueRequest { MoveAfterId = 3 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/reorder",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"move_after_id\":3}", sentBody);
    }

    [Theory]
    [InlineData(true, "closed_by")]
    [InlineData(false, "related_merge_requests")]
    public async Task MergeRequestListings_BuildTheirOwnRoutes_AndDeserializeMergeRequests(bool closedBy,
        string expectedSegment)
    {
        const string Json = """
                            [
                              {
                                "id": 5,
                                "iid": 2,
                                "project_id": 42,
                                "title": "Add the README",
                                "state": "merged",
                                "source_branch": "readme",
                                "target_branch": "main",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/merge_requests/2"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = Responds(Json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        List<GitLabMergeRequest> mergeRequests = [];

        if (closedBy)
        {
            await foreach (GitLabMergeRequest mergeRequest in
                           repository.ListClosedByAsync(42, 6, TestContext.Current.CancellationToken))
            {
                mergeRequests.Add(mergeRequest);
            }
        }
        else
        {
            await foreach (GitLabMergeRequest mergeRequest in
                           repository.ListRelatedMergeRequestsAsync(42, 6, TestContext.Current.CancellationToken))
            {
                mergeRequests.Add(mergeRequest);
            }
        }

        Assert.Equal($"https://gitlab.example/api/v4/projects/42/issues/6/{expectedSegment}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("merged", Assert.Single(mergeRequests).State);
    }

    [Fact]
    public async Task ListParticipantsAsync_BuildsTheParticipantsRoute_AndDeserializesUsers()
    {
        using StubHttpMessageHandler handler = Responds($"[{BasicUserJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        List<GitLabUser> participants = [];
        await foreach (GitLabUser user in
                       repository.ListParticipantsAsync(42, 6, TestContext.Current.CancellationToken))
        {
            participants.Add(user);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/participants",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("root", Assert.Single(participants).Username);
    }

    [Fact]
    public async Task GetUserAgentDetailAsync_DeserializesTheSpamCheckRecord()
    {
        const string Json = """
                            {
                              "user_agent": "Mozilla/5.0",
                              "ip_address": "127.0.0.1",
                              "akismet_submitted": false
                            }
                            """;

        using StubHttpMessageHandler handler = Responds(Json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        GitLabUserAgentDetail detail =
            await repository.GetUserAgentDetailAsync(42, 6, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/user_agent_detail",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Mozilla/5.0", detail.UserAgent);
        Assert.Equal("127.0.0.1", detail.IpAddress);
        Assert.False(detail.AkismetSubmitted);
    }

    [Fact]
    public async Task ListLinksAsync_DeserializesTheLinkFields_GitLabGraftsOntoTheLinkedIssue()
    {
        const string Json = """
                            [
                              {
                                "id": 84,
                                "iid": 14,
                                "project_id": 42,
                                "title": "Blocked by infra",
                                "state": "opened",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/issues/14",
                                "issue_link_id": 100,
                                "link_type": "is_blocked_by",
                                "link_created_at": "2024-05-06T07:08:09.000Z",
                                "link_updated_at": "2024-05-07T07:08:09.000Z"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = Responds(Json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        List<GitLabIssue> linked = [];
        await foreach (GitLabIssue issue in repository.ListLinksAsync(42, 6, TestContext.Current.CancellationToken))
        {
            linked.Add(issue);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/links",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabIssue only = Assert.Single(linked);
        Assert.Equal(100, only.IssueLinkId);
        Assert.Equal("is_blocked_by", only.LinkType);
        Assert.Equal(new DateTimeOffset(2024, 5, 6, 7, 8, 9, TimeSpan.Zero), only.LinkCreatedAt);
    }

    [Fact]
    public async Task CreateLinkAsync_PostsTheRelation_AndDeserializesBothEndpointsOfTheLink()
    {
        string json = $$"""
                        {
                          "id": 100,
                          "link_type": "blocks",
                          "source_issue": {{MinimalIssueJson}},
                          "target_issue": {{MinimalIssueJson}}
                        }
                        """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        GitLabIssueLink link = await repository.CreateLinkAsync(
            42,
            6,
            new CreateIssueLinkRequest
            {
                TargetProjectId = "gitlab-org/gitlab", TargetIssueIid = 14, LinkType = GitLabIssueLinkType.Blocks
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/links",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"target_project_id\":\"gitlab-org/gitlab\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"target_issue_iid\":14", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"link_type\":\"blocks\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(100, link.Id);
        Assert.Equal("blocks", link.LinkType);
        Assert.Equal(76, link.TargetIssue?.Id);
    }

    [Fact]
    public async Task GetLinkAsync_AddsTheLinkIdToTheRoute()
    {
        const string Json = """{ "id": 100, "link_type": "relates_to" }""";

        using StubHttpMessageHandler handler = Responds(Json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        GitLabIssueLink link = await repository.GetLinkAsync(42, 6, 100, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/links/100",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("relates_to", link.LinkType);
    }

    [Fact]
    public async Task DeleteLinkAsync_SendsDeleteToTheLinkRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.DeleteLinkAsync(42, 6, 100, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/links/100",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetTimeStatsAsync_DeserializesTheTrackingTotals()
    {
        using StubHttpMessageHandler handler = Responds(TimeStatsJson);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        GitLabTimeStats stats = await repository.GetTimeStatsAsync(42, 6, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/time_stats",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(3600, stats.TimeEstimate);
        Assert.Equal(1800, stats.TotalTimeSpent);
        Assert.Equal("1h", stats.HumanTimeEstimate);
    }

    [Fact]
    public async Task SetTimeEstimateAsync_PostsTheDuration()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(TimeStatsJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.SetTimeEstimateAsync(42, 6, new SetIssueTimeEstimateRequest { Duration = "3h30m" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/time_estimate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"duration\":\"3h30m\"}", sentBody);
    }

    [Fact]
    public async Task AddSpentTimeAsync_PostsTheDuration()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(TimeStatsJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.AddSpentTimeAsync(42, 6, new AddIssueSpentTimeRequest { Duration = "-30m" },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/add_spent_time",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"duration\":\"-30m\"}", sentBody);
    }

    [Theory]
    [InlineData(true, "reset_time_estimate")]
    [InlineData(false, "reset_spent_time")]
    public async Task ResetEndpoints_PostWithNoBody(bool estimate, string expectedSegment)
    {
        using StubHttpMessageHandler handler = Responds(TimeStatsJson, HttpStatusCode.Created);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        GitLabTimeStats stats = estimate
            ? await repository.ResetTimeEstimateAsync(42, 6, TestContext.Current.CancellationToken)
            : await repository.ResetSpentTimeAsync(42, 6, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/projects/42/issues/6/{expectedSegment}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(3600, stats.TimeEstimate);
    }

    [Fact]
    public async Task GetStatisticsAsync_BuildsTheInstanceRoute_AndDeserializesTheNestedCounts()
    {
        const string Json = """
                            { "statistics": { "counts": { "all": 20, "closed": 5, "opened": 15 } } }
                            """;

        using StubHttpMessageHandler handler = Responds(Json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        GitLabIssueStatistics statistics = await repository.GetStatisticsAsync(
            new IssueStatisticsOptions { Labels = BugLabel, Scope = GitLabIssueScope.CreatedByMe },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/issues_statistics?labels=bug&scope=created_by_me",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(20, statistics.Statistics?.Counts?.All);
        Assert.Equal(5, statistics.Statistics?.Counts?.Closed);
        Assert.Equal(15, statistics.Statistics?.Counts?.Opened);
    }

    [Fact]
    public async Task GetProjectStatisticsAsync_BuildsTheProjectScopedRoute()
    {
        const string Json = """{ "statistics": { "counts": { "all": 1, "closed": 0, "opened": 1 } } }""";

        using StubHttpMessageHandler handler = Responds(Json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.GetProjectStatisticsAsync("gitlab-org/gitlab", null, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues_statistics",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetProjectStatisticsAsync_ProjectsFiltersOntoTheQueryString()
    {
        const string Json = """{ "statistics": { "counts": { "all": 1, "closed": 0, "opened": 1 } } }""";

        using StubHttpMessageHandler handler = Responds(Json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.GetProjectStatisticsAsync(
            "gitlab-org/gitlab",
            new IssueStatisticsOptions { Labels = BugLabel, AuthorId = 9, Confidential = false },
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues_statistics"
            + "?labels=bug&author_id=9&confidential=false",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetGroupStatisticsAsync_BuildsTheGroupScopedRoute()
    {
        const string Json = """{ "statistics": { "counts": { "all": 1, "closed": 0, "opened": 1 } } }""";

        using StubHttpMessageHandler handler = Responds(Json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.GetGroupStatisticsAsync("gitlab-org/sub", null, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsub/issues_statistics",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetGroupStatisticsAsync_ProjectsFiltersOntoTheQueryString()
    {
        const string Json = """{ "statistics": { "counts": { "all": 1, "closed": 0, "opened": 1 } } }""";

        using StubHttpMessageHandler handler = Responds(Json);
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.GetGroupStatisticsAsync(
            "gitlab-org/sub",
            new IssueStatisticsOptions { Scope = GitLabIssueScope.AssignedToMe, Weight = "None" },
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsub/issues_statistics"
            + "?scope=assigned_to_me&weight=None",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListMetricImagesAsync_BuildsTheMetricImagesRoute_AndDeserializesTheImages()
    {
        using StubHttpMessageHandler handler = Responds($"[{MetricImageJson}]");
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        List<GitLabMetricImage> images = [];
        await foreach (GitLabMetricImage image in
                       repository.ListMetricImagesAsync(42, 6, TestContext.Current.CancellationToken))
        {
            images.Add(image);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/metric_images",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabMetricImage only = Assert.Single(images);
        Assert.Equal(23, only.Id);
        Assert.Equal("cpu.png", only.Filename);
        Assert.Equal(new Uri("https://grafana.example/d/abc"), only.Url);
        Assert.Equal("CPU saturation", only.Caption);
    }

    [Fact]
    public async Task UploadMetricImageAsync_SendsMultipartFormData_WithTheFileAndTheFormFields()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(MetricImageJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        using MemoryStream content = new("PNG-BYTES"u8.ToArray());

        GitLabMetricImage image = await repository.UploadMetricImageAsync(
            42,
            6,
            new GitLabFileUpload { Content = content, FileName = "cpu.png", ContentType = "image/png" },
            new Uri("https://grafana.example/d/abc"),
            "CPU saturation",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/metric_images",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.StartsWith("multipart/form-data", handler.LastRequest?.Content?.Headers.ContentType?.MediaType
                                                 ?? string.Empty, StringComparison.Ordinal);

        string body = (sentBody ?? string.Empty).Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=file", body, StringComparison.Ordinal);
        Assert.Contains("filename=cpu.png", body, StringComparison.Ordinal);
        Assert.Contains("PNG-BYTES", body, StringComparison.Ordinal);
        Assert.Contains("name=url", body, StringComparison.Ordinal);
        Assert.Contains("https://grafana.example/d/abc", body, StringComparison.Ordinal);
        Assert.Contains("name=url_text", body, StringComparison.Ordinal);
        Assert.Contains("CPU saturation", body, StringComparison.Ordinal);

        // The upload stream is borrowed, never owned - the caller's using block still means what it says.
        Assert.True(content.CanRead);
        Assert.Equal(23, image.Id);
    }

    [Fact]
    public async Task UploadMetricImageAsync_WithoutOptionalFields_SendsOnlyTheFilePart()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(MetricImageJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        using MemoryStream content = new("PNG-BYTES"u8.ToArray());

        await repository.UploadMetricImageAsync(
            42,
            6,
            new GitLabFileUpload { Content = content, FileName = "cpu.png" },
            cancellationToken: TestContext.Current.CancellationToken);

        string body = (sentBody ?? string.Empty).Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=file", body, StringComparison.Ordinal);
        Assert.DoesNotContain("name=url", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AuthorizeMetricImageUploadAsync_PostsToTheAuthorizeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.AuthorizeMetricImageUploadAsync(42, 6, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/metric_images/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateMetricImageAsync_PutsTheLinkAndCaption()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(MetricImageJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.UpdateMetricImageAsync(
            42,
            6,
            23,
            new UpdateMetricImageRequest { Url = new Uri("https://grafana.example/d/abc"), Caption = "CPU" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/metric_images/23",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"url\":\"https://grafana.example/d/abc\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"url_text\":\"CPU\"", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteMetricImageAsync_SendsDeleteToTheMetricImageRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        IssuesRepository repository = new(connection);

        await repository.DeleteMetricImageAsync(42, 6, 23, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/issues/6/metric_images/23",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}