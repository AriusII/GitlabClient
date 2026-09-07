using System.Text.Json;

using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Usage data, sitting between the public
///     <c>IUsageDataClient</c> controller and <c>IUsageDataRepository</c>'s raw GitLab access. Mirrors
///     the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IUsageDataService
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