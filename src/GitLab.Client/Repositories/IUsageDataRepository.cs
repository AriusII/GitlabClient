using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Usage data resource - Service Ping (<c>/usage_data/*</c>) and the
///     internal event-tracking API it shares a route prefix with. Builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IUsageDataService), typeof(IUsageDataClient))]
internal interface IUsageDataRepository
{
    Task IncrementCounterAsync(UsageDataEventRequest request, CancellationToken cancellationToken = default);

    Task IncrementUniqueUsersAsync(UsageDataEventRequest request, CancellationToken cancellationToken = default);

    Task<JsonElement> GetNonSqlMetricsAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> GetQueriesAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> GetServicePingAsync(CancellationToken cancellationToken = default);

    Task TrackEventAsync(TrackEventRequest request, CancellationToken cancellationToken = default);

    Task TrackEventsAsync(TrackEventsRequest request, CancellationToken cancellationToken = default);

    Task<JsonElement> GetMetricDefinitionsAsync(bool? includePaths = null,
        CancellationToken cancellationToken = default);
}