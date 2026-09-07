using System.Text.Json;

using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Sidekiq, sitting between the public <c>ISidekiqClient</c>
///     controller and <c>ISidekiqRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ISidekiqService
{
    Task DeleteQueueJobsAsync(string queueName, SidekiqQueueJobDeleteOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetQueueMetricsAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> GetProcessMetricsAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> GetJobStatsAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> GetCompoundMetricsAsync(CancellationToken cancellationToken = default);
}