using System.Text.Json;

using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's Service Ping / usage-telemetry surface (<c>/usage_data/*</c>), including the
///     internal event-tracking API it shares a route prefix with.
///     <para>
///         Service Ping's own payloads (<see cref="GetServicePingAsync" />, <see cref="GetNonSqlMetricsAsync" />,
///         <see cref="GetQueriesAsync" />, <see cref="GetMetricDefinitionsAsync" />) are famously large and
///         semi-structured, and GitLab's OpenAPI document declares no response schema for any of them - they
///         are surfaced as a raw <see cref="JsonElement" /> rather than a guessed DTO with dozens of
///         speculative properties.
///     </para>
///     <para>Most of these endpoints require administrator access on the target instance.</para>
/// </summary>
public interface IUsageDataClient
{
    /// <summary>Increments a Service Ping counter by one for the named event.</summary>
    Task IncrementCounterAsync(UsageDataEventRequest request, CancellationToken cancellationToken = default);

    /// <summary>Records that the current user triggered the named event, for Service Ping's unique-user counters.</summary>
    Task IncrementUniqueUsersAsync(UsageDataEventRequest request, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the definitions and current values of Service Ping's non-SQL metrics.</summary>
    Task<JsonElement> GetNonSqlMetricsAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves the raw SQL text of every query Service Ping runs to build its payload.</summary>
    Task<JsonElement> GetQueriesAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves the full Service Ping payload the instance would submit to GitLab.</summary>
    Task<JsonElement> GetServicePingAsync(CancellationToken cancellationToken = default);

    /// <summary>Tracks a single internal GitLab event, e.g. for product-usage analytics.</summary>
    Task TrackEventAsync(TrackEventRequest request, CancellationToken cancellationToken = default);

    /// <summary>Tracks a batch of internal GitLab events in one call.</summary>
    Task TrackEventsAsync(TrackEventsRequest request, CancellationToken cancellationToken = default);

    /// <summary>Downloads every Service Ping metric definition known to the instance.</summary>
    Task<JsonElement> GetMetricDefinitionsAsync(bool? includePaths = null,
        CancellationToken cancellationToken = default);
}