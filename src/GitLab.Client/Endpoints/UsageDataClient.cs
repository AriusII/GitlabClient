using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class UsageDataClient(IGitLabApiConnection connection) : IUsageDataClient
{
    public Task IncrementCounterAsync(UsageDataEventRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("usage_data").Literal("increment_counter").Build(),
            request,
            GitLabJsonContext.Default.UsageDataEventRequest,
            cancellationToken);
    }

    public Task IncrementUniqueUsersAsync(UsageDataEventRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("usage_data").Literal("increment_unique_users").Build(),
            request,
            GitLabJsonContext.Default.UsageDataEventRequest,
            cancellationToken);
    }

    public Task<JsonElement> GetNonSqlMetricsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("usage_data").Literal("non_sql_metrics").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetQueriesAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("usage_data").Literal("queries").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetServicePingAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("usage_data").Literal("service_ping").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task TrackEventAsync(TrackEventRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("usage_data").Literal("track_event").Build(),
            request,
            GitLabJsonContext.Default.TrackEventRequest,
            cancellationToken);
    }

    public Task TrackEventsAsync(TrackEventsRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("usage_data").Literal("track_events").Build(),
            request,
            GitLabJsonContext.Default.TrackEventsRequest,
            cancellationToken);
    }

    public Task<JsonElement> GetMetricDefinitionsAsync(bool? includePaths = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("usage_data").Literal("metric_definitions")
                .Query("include_paths", includePaths).Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }
}