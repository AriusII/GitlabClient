using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class UsageDataEndpointTests
{
    [Fact]
    public async Task IncrementCounterAsync_PostsTheEventName_AndSendsNoResponseBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsageDataClient repository = new(connection);

        await repository.IncrementCounterAsync(new UsageDataEventRequest { Event = "i_analytics_dev_ops_score" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/usage_data/increment_counter",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"event":"i_analytics_dev_ops_score"}""", sentBody);
    }

    [Fact]
    public async Task IncrementUniqueUsersAsync_PostsToTheUniqueUsersRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsageDataClient repository = new(connection);

        await repository.IncrementUniqueUsersAsync(new UsageDataEventRequest { Event = "users_visiting_pipelines" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/usage_data/increment_unique_users",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"event":"users_visiting_pipelines"}""", sentBody);
    }

    [Fact]
    public async Task GetNonSqlMetricsAsync_GetsTheRoute_AndReturnsTheRawEnvelope()
    {
        const string Json = """{"counts_28d":{"ci_builds":10}}""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsageDataClient repository = new(connection);

        JsonElement result = await repository.GetNonSqlMetricsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/usage_data/non_sql_metrics",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(JsonValueKind.Object, result.ValueKind);
        Assert.Equal(10, result.GetProperty("counts_28d").GetProperty("ci_builds").GetInt32());
    }

    [Fact]
    public async Task GetQueriesAsync_GetsTheRoute()
    {
        const string Json = """{"counts.issues":"SELECT COUNT(*) FROM issues"}""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsageDataClient repository = new(connection);

        JsonElement result = await repository.GetQueriesAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/usage_data/queries", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("SELECT COUNT(*) FROM issues", result.GetProperty("counts.issues").GetString());
    }

    [Fact]
    public async Task GetServicePingAsync_GetsTheRoute()
    {
        const string Json = """{"uuid":"abc-123","edition":"EE"}""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsageDataClient repository = new(connection);

        JsonElement result = await repository.GetServicePingAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/usage_data/service_ping",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("abc-123", result.GetProperty("uuid").GetString());
    }

    [Fact]
    public async Task TrackEventAsync_PostsTheFullEventBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsageDataClient repository = new(connection);

        TrackEventRequest request = new()
        {
            Event = "code_suggestion_shown_in_ide",
            NamespaceId = 9970,
            ProjectId = 42,
            ProjectPath = "gitlab-org/gitlab",
            AdditionalProperties = new Dictionary<string, JsonElement>
            {
                ["language"] = JsonDocument.Parse("\"csharp\"").RootElement
            },
            SendToSnowplow = true
        };

        await repository.TrackEventAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/usage_data/track_event",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """
            {"event":"code_suggestion_shown_in_ide","namespace_id":9970,"project_id":42,"project_path":"gitlab-org/gitlab","additional_properties":{"language":"csharp"},"send_to_snowplow":true}
            """,
            sentBody);
    }

    [Fact]
    public async Task TrackEventsAsync_PostsTheEventsArray()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsageDataClient repository = new(connection);

        TrackEventsRequest request = new()
        {
            Events =
            [
                new TrackEventRequest { Event = "first_event" },
                new TrackEventRequest { Event = "second_event", ProjectId = 7 }
            ]
        };

        await repository.TrackEventsAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/usage_data/track_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"events":[{"event":"first_event"},{"event":"second_event","project_id":7}]}""",
            sentBody);
    }

    [Fact]
    public async Task GetMetricDefinitionsAsync_OmitsQuery_WhenIncludePathsNotGiven()
    {
        const string Json = """[{"key_path":"counts.issues","name":"Issues"}]""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsageDataClient repository = new(connection);

        JsonElement result = await repository.GetMetricDefinitionsAsync(
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/usage_data/metric_definitions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(JsonValueKind.Array, result.ValueKind);
    }

    [Fact]
    public async Task GetMetricDefinitionsAsync_SendsIncludePaths_WhenGiven()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsageDataClient repository = new(connection);

        await repository.GetMetricDefinitionsAsync(true, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/usage_data/metric_definitions?include_paths=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task IncrementCounterAsync_OnBadRequest_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": "event parameter is required" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        UsageDataClient repository = new(connection);

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.IncrementCounterAsync(new UsageDataEventRequest { Event = "" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("event parameter is required", exception.Message);
    }
}