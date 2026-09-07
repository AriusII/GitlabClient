using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class AnalyticsRepositoryTests
{
    [Fact]
    public async Task GetGroupActivityIssuesCountAsync_BuildsRoute_AndSendsGroupPathAsQuery()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"issues_count":42}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AnalyticsRepository repository = new(connection);

        GitLabGroupIssuesCount result = await repository.GetGroupActivityIssuesCountAsync("gitlab-org/gitlab",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/analytics/group_activity/issues_count?group_path=gitlab-org%2Fgitlab",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(42, result.IssuesCount);
    }

    [Fact]
    public async Task GetGroupActivityMergeRequestsCountAsync_BuildsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"merge_requests_count":7}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AnalyticsRepository repository = new(connection);

        GitLabGroupMergeRequestsCount result = await repository.GetGroupActivityMergeRequestsCountAsync("acme",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/analytics/group_activity/merge_requests_count?group_path=acme",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(7, result.MergeRequestsCount);
    }

    [Fact]
    public async Task GetGroupActivityNewMembersCountAsync_BuildsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"new_members_count":3}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AnalyticsRepository repository = new(connection);

        GitLabGroupNewMembersCount result = await repository.GetGroupActivityNewMembersCountAsync("acme",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/analytics/group_activity/new_members_count?group_path=acme",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(3, result.NewMembersCount);
    }

    [Fact]
    public async Task ListDeploymentFrequencyAsync_BuildsRoute_WithRequiredAndOptionalQueryParameters()
    {
        const string Json = """
                            [
                              { "value": "2", "from": "2024-01-01", "to": "2024-01-02" },
                              { "value": "5", "from": "2024-01-02", "to": "2024-01-03" }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AnalyticsRepository repository = new(connection);

        List<GitLabDeploymentFrequency> results = [];
        await foreach (GitLabDeploymentFrequency item in repository.ListDeploymentFrequencyAsync(
                           "gitlab-org/gitlab", "production", "2024-01-01",
                           new DeploymentFrequencyListOptions { To = "2024-02-01", Interval = "daily" },
                           TestContext.Current.CancellationToken))
        {
            results.Add(item);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/analytics/deployment_frequency" +
            "?environment=production&from=2024-01-01&to=2024-02-01&interval=daily",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(2, results.Count);
        Assert.Equal("2", results[0].Value);
        Assert.Equal("2024-01-02", results[0].To);
    }

    [Fact]
    public async Task ListCodeReviewAnalyticsAsync_BuildsQueryString_WithNotFilters_AndDeserializesUnknownNestedShapes()
    {
        const string Json = """
                            [
                              {
                                "id": 501,
                                "iid": 12,
                                "project_id": 7,
                                "title": "Add analytics support",
                                "description": "Wraps the analytics endpoints",
                                "state": "opened",
                                "created_at": "2024-03-01T10:00:00Z",
                                "updated_at": "2024-03-02T09:00:00Z",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/merge_requests/12",
                                "milestone": { "id": 9, "title": "19.4" },
                                "author": { "id": 1, "username": "root" },
                                "approved_by": [{ "id": 2, "username": "reviewer" }],
                                "notes_count": 4,
                                "review_time": 3600,
                                "diff_stats": "+120 -30"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AnalyticsRepository repository = new(connection);

        CodeReviewAnalyticsListOptions options = new()
        {
            LabelName = ["backend", "urgent"], NotLabelName = ["wontfix"], MilestoneTitle = "19.4", PerPage = 20
        };

        List<GitLabCodeReviewAnalyticsItem> results = [];
        await foreach (GitLabCodeReviewAnalyticsItem entry in
                       repository.ListCodeReviewAnalyticsAsync(7, options, TestContext.Current.CancellationToken))
        {
            results.Add(entry);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/analytics/code_review?project_id=7&label_name=backend,urgent" +
            "&milestone_title=19.4&not[label_name]=wontfix&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabCodeReviewAnalyticsItem item = Assert.Single(results);
        Assert.Equal(501, item.Id);
        Assert.Equal(12, item.Iid);
        Assert.Equal("opened", item.State);
        Assert.Equal("+120 -30", item.DiffStats);
        Assert.Equal(JsonValueKind.Object, item.Milestone?.ValueKind);
        Assert.Equal("19.4", item.Milestone?.GetProperty("title").GetString());
        Assert.Equal(JsonValueKind.Array, item.ApprovedBy?.ValueKind);
    }

    [Fact]
    public async Task GetGroupDoraMetricsAsync_BuildsRoute_WithMetricAndFilters_AndReturnsRawElement()
    {
        const string Json = """[{"date":"2024-01-01","value":3}]""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AnalyticsRepository repository = new(connection);

        DoraMetricsOptions options = new()
        {
            StartDate = "2024-01-01",
            EndDate = "2024-01-31",
            Interval = "monthly",
            EnvironmentTiers = ["production", "staging"]
        };

        JsonElement result = await repository.GetGroupDoraMetricsAsync("parent-group/subgroup",
            "deployment_frequency", options, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/dora/metrics" +
            "?metric=deployment_frequency&start_date=2024-01-01&end_date=2024-01-31&interval=monthly" +
            "&environment_tiers=production,staging",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(JsonValueKind.Array, result.ValueKind);
        Assert.Equal(3, result[0].GetProperty("value").GetInt32());
    }

    [Fact]
    public async Task GetProjectDoraMetricsAsync_BuildsRoute_WithOnlyTheRequiredMetric()
    {
        const string Json = """[{"date":"2024-01-01","value":0.5}]""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AnalyticsRepository repository = new(connection);

        JsonElement result = await repository.GetProjectDoraMetricsAsync(42, "change_failure_rate",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/dora/metrics?metric=change_failure_rate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(JsonValueKind.Array, result.ValueKind);
    }
}