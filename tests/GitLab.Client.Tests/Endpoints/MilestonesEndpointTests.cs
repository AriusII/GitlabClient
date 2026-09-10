using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class MilestonesEndpointTests
{
    private const string GroupMilestoneJson = """
                                              {
                                                "id": 12,
                                                "iid": 3,
                                                "group_id": 9970,
                                                "title": "Q1 hardening",
                                                "description": "Cross-project milestone",
                                                "state": "active",
                                                "created_at": "2026-01-01T00:00:00Z",
                                                "updated_at": "2026-02-01T00:00:00Z",
                                                "due_date": "2026-03-31",
                                                "start_date": "2026-01-01",
                                                "expired": false,
                                                "web_url": "https://gitlab.example/groups/mygroup/-/milestones/3"
                                              }
                                              """;

    [Fact]
    public async Task GetAsync_BuildsExpectedRoute_AndDeserializesMilestone()
    {
        const string Json = """
                            {
                              "id": 42,
                              "iid": 5,
                              "project_id": 123,
                              "title": "Release 1.0",
                              "description": "First stable release",
                              "state": "active",
                              "created_at": "2026-01-01T00:00:00Z",
                              "updated_at": "2026-02-01T00:00:00Z",
                              "due_date": "2026-03-15",
                              "start_date": "2026-01-15",
                              "expired": false,
                              "web_url": "https://gitlab.example/groups/mygroup/-/milestones/5"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        GitLabMilestone milestone =
            await repository.GetAsync(ProjectId.FromId(123), 42, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/123/milestones/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(42, milestone.Id);
        Assert.Equal(5, milestone.Iid);
        Assert.Equal(123, milestone.ProjectId);
        Assert.Null(milestone.GroupId);
        Assert.Equal("Release 1.0", milestone.Title);
        Assert.Equal("First stable release", milestone.Description);
        Assert.Equal("active", milestone.State);
        Assert.Equal(new DateOnly(2026, 3, 15), milestone.DueDate);
        Assert.Equal(new DateOnly(2026, 1, 15), milestone.StartDate);
        Assert.False(milestone.Expired);
        Assert.Equal(new Uri("https://gitlab.example/groups/mygroup/-/milestones/5"), milestone.WebUrl);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Milestone Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(ProjectId.FromId(1), 99, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Milestone Not Found", exception.Message);
    }

    [Fact]
    public async Task ListAsync_BuildsExpectedRouteWithQuery_AndStreamsAllItems()
    {
        const string Json = """
                            [
                              { "id": 1, "iid": 1, "title": "M1", "state": "active", "web_url": "https://gitlab.example/groups/mygroup/-/milestones/1" },
                              { "id": 2, "iid": 2, "title": "M2", "state": "active", "web_url": "https://gitlab.example/groups/mygroup/-/milestones/2" }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        MilestoneListOptions options = new() { State = GitLabMilestoneStateFilter.Active, PerPage = 50 };
        List<GitLabMilestone> milestones = new();

        await foreach (GitLabMilestone milestone in repository.ListAsync(ProjectId.FromId(123), options,
                           TestContext.Current.CancellationToken))
        {
            milestones.Add(milestone);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/123/milestones?state=active&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, milestones.Count);
        Assert.Equal("M1", milestones[0].Title);
        Assert.Equal("M2", milestones[1].Title);
    }

    [Fact]
    public async Task ListAsync_ProjectionsTheWholeProjectFilterSet()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        MilestoneListOptions options = new()
        {
            State = GitLabMilestoneStateFilter.Closed,
            Iids = [3, 5],
            Title = "Release 1.0",
            Search = "release",
            IncludeAncestors = true,
            UpdatedAfter = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        await foreach (GitLabMilestone _ in repository.ListAsync("gitlab-org/gitlab", options,
                           TestContext.Current.CancellationToken))
        {
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.StartsWith("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/milestones?", requestUri);
        Assert.Contains("state=closed", requestUri);
        Assert.Contains("iids=3,5", requestUri);
        Assert.Contains("title=Release%201.0", requestUri);
        Assert.Contains("search=release", requestUri);
        Assert.Contains("include_ancestors=true", requestUri);
        Assert.Contains("updated_after=2026-01-01T00:00:00Z", requestUri);
    }

    [Fact]
    public async Task CreateAsync_PostsTheMilestoneBody_WithIsoDates()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(GroupMilestoneJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        CreateMilestoneRequest request = new()
        {
            Title = "Release 1.0",
            Description = "First stable release",
            StartDate = new DateOnly(2026, 1, 15),
            DueDate = new DateOnly(2026, 3, 15)
        };

        await repository.CreateAsync(123, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/123/milestones",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // GitLab accepts only the plain %Y-%m-%d form here, never a full timestamp.
        Assert.Equal(
            """{"title":"Release 1.0","description":"First stable release","due_date":"2026-03-15","start_date":"2026-01-15"}""",
            sentBody);
    }

    [Fact]
    public async Task UpdateAsync_PutsTheStateEventVerb()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GroupMilestoneJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        UpdateMilestoneRequest request = new() { StateEvent = GitLabMilestoneStateEvent.Close };

        await repository.UpdateAsync("gitlab-org/gitlab", 42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/milestones/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"state_event":"close"}""", sentBody);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheMilestoneRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        await repository.DeleteAsync(123, 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/123/milestones/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task PromoteAsync_PostsToThePromoteRoute_WithNoBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        await repository.PromoteAsync(123, 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/123/milestones/42/promote",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task ListIssuesAsync_BuildsTheMilestoneIssuesRoute_AndDeserializesIssues()
    {
        const string Json = """
                            [
                              {
                                "id": 76,
                                "iid": 6,
                                "project_id": 123,
                                "title": "Fix the burndown",
                                "state": "opened",
                                "labels": ["type::bug"],
                                "web_url": "https://gitlab.example/mygroup/myproject/-/issues/6"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        List<GitLabIssue> issues = new();
        await foreach (GitLabIssue issue in repository.ListIssuesAsync(123, 42,
                           new MilestoneIssuableListOptions { PerPage = 30 }, TestContext.Current.CancellationToken))
        {
            issues.Add(issue);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/123/milestones/42/issues?per_page=30",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabIssue single = Assert.Single(issues);
        Assert.Equal(76, single.Id);
        Assert.Equal("Fix the burndown", single.Title);
        Assert.Equal("type::bug", Assert.Single(single.Labels!));
    }

    [Fact]
    public async Task ListMergeRequestsAsync_BuildsTheMilestoneMergeRequestsRoute()
    {
        const string Json = """
                            [
                              {
                                "id": 55,
                                "iid": 9,
                                "project_id": 123,
                                "title": "Rework the chart",
                                "state": "opened",
                                "source_branch": "chart-rework",
                                "target_branch": "main",
                                "web_url": "https://gitlab.example/mygroup/myproject/-/merge_requests/9"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        List<GitLabMergeRequest> mergeRequests = new();
        await foreach (GitLabMergeRequest mergeRequest in repository.ListMergeRequestsAsync("gitlab-org/gitlab", 42,
                           new MilestoneIssuableListOptions { PerPage = 15 }, TestContext.Current.CancellationToken))
        {
            mergeRequests.Add(mergeRequest);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/milestones/42/merge_requests?per_page=15",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("chart-rework", Assert.Single(mergeRequests).SourceBranch);
    }

    [Fact]
    public async Task ListBurndownEventsAsync_BuildsTheBurndownRoute_AndDeserializesEvents()
    {
        const string Json = """
                            [
                              { "created_at": "2026-02-03T10:15:00Z", "weight": 3, "action": "created" },
                              { "created_at": "2026-02-05T09:00:00Z", "weight": 3, "action": "closed" }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        List<GitLabBurndownEvent> events = new();
        await foreach (GitLabBurndownEvent burndownEvent in repository.ListBurndownEventsAsync(123, 42,
                           TestContext.Current.CancellationToken))
        {
            events.Add(burndownEvent);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/123/milestones/42/burndown_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, events.Count);
        Assert.Equal("created", events[0].Action);
        Assert.Equal(3, events[0].Weight);
        Assert.Equal(new DateTimeOffset(2026, 2, 5, 9, 0, 0, TimeSpan.Zero), events[1].CreatedAt);
        Assert.Equal("closed", events[1].Action);
    }

    [Fact]
    public async Task GetForGroupAsync_BuildsTheGroupRoute_AndReadsGroupId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(GroupMilestoneJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        GitLabMilestone milestone =
            await repository.GetForGroupAsync("parent-group/subgroup", 12, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/milestones/12",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(12, milestone.Id);
        Assert.Equal(9970, milestone.GroupId);
        Assert.Null(milestone.ProjectId);
        Assert.Equal("Q1 hardening", milestone.Title);
    }

    [Fact]
    public async Task ListForGroupAsync_ProjectsTheGroupOnlyDescendantsFilter()
    {
        string json = $"[{GroupMilestoneJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        GroupMilestoneListOptions options = new()
        {
            State = GitLabMilestoneStateFilter.Active,
            IncludeAncestors = true,
            IncludeDescendants = true,
            PerPage = 20
        };

        List<GitLabMilestone> milestones = new();
        await foreach (GitLabMilestone milestone in repository.ListForGroupAsync(9970, options,
                           TestContext.Current.CancellationToken))
        {
            milestones.Add(milestone);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9970/milestones"
            + "?state=active&include_ancestors=true&include_descendants=true&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Q1 hardening", Assert.Single(milestones).Title);
    }

    [Fact]
    public async Task CreateForGroupAsync_PostsToTheGroupRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(GroupMilestoneJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        CreateMilestoneRequest request = new() { Title = "Q1 hardening" };

        GitLabMilestone milestone =
            await repository.CreateForGroupAsync(9970, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/milestones",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"title":"Q1 hardening"}""", sentBody);
        Assert.Equal(9970, milestone.GroupId);
    }

    [Fact]
    public async Task UpdateForGroupAsync_PutsToTheGroupMilestoneRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GroupMilestoneJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        UpdateMilestoneRequest request = new()
        {
            Title = "Q1 hardening",
            StateEvent = GitLabMilestoneStateEvent.Activate,
            DueDate = new DateOnly(2026, 3, 31)
        };

        await repository.UpdateForGroupAsync(9970, 12, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/milestones/12",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"title":"Q1 hardening","state_event":"activate","due_date":"2026-03-31"}""", sentBody);
    }

    [Fact]
    public async Task DeleteForGroupAsync_SendsDeleteToTheGroupMilestoneRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        await repository.DeleteForGroupAsync("parent-group/subgroup", 12, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/milestones/12",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListIssuesForGroupAsync_BuildsTheGroupMilestoneIssuesRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        await foreach (GitLabIssue _ in repository.ListIssuesForGroupAsync(9970, 12,
                           new MilestoneIssuableListOptions { PerPage = 25 }, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/milestones/12/issues?per_page=25",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListMergeRequestsForGroupAsync_BuildsTheGroupMilestoneMergeRequestsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        await foreach (GitLabMergeRequest _ in repository.ListMergeRequestsForGroupAsync(9970, 12,
                           new MilestoneIssuableListOptions { PerPage = 10 }, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/milestones/12/merge_requests?per_page=10",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListBurndownEventsForGroupAsync_BuildsTheGroupBurndownRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        await foreach (GitLabBurndownEvent _ in repository.ListBurndownEventsForGroupAsync("parent-group/subgroup",
                           12, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/milestones/12/burndown_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListBurndownEventsAsync_WithoutTheLicensedFeature_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MilestonesClient repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(async () =>
        {
            await foreach (GitLabBurndownEvent _ in repository
                               .ListBurndownEventsAsync(123, 42, TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }
}