using System.Text.Json;

using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's Sidekiq admin APIs (<c>/admin/sidekiq/queues/:queue_name</c>,
///     <c>/sidekiq/queue_metrics</c>, <c>/sidekiq/process_metrics</c>, <c>/sidekiq/job_stats</c>,
///     <c>/sidekiq/compound_metrics</c>) - draining a stuck job queue by metadata, and the background
///     job processing metrics GitLab's own admin area renders.
///     <para>
///         Every method here requires instance administrator access; GitLab answers <c>401</c>/<c>403</c>
///         to anyone else even though the metrics routes do not carry an <c>/admin</c> prefix. None of
///         the four metrics endpoints declare a response schema in GitLab's OpenAPI document, so their
///         answers come back as a raw <see cref="JsonElement" /> rather than as an invented DTO.
///     </para>
/// </summary>
public interface ISidekiqClient
{
    /// <summary>
    ///     Deletes every job in <paramref name="queueName" /> whose metadata matches every filter set on
    ///     <paramref name="options" />. GitLab refuses the request unless at least one filter is set, to
    ///     stop an accidental whole-queue wipe.
    /// </summary>
    Task DeleteQueueJobsAsync(string queueName, SidekiqQueueJobDeleteOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists backlog size and latency for every Sidekiq job queue.</summary>
    Task<JsonElement> GetQueueMetricsAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists hostname, process id, queues and concurrency for every registered Sidekiq worker process.</summary>
    Task<JsonElement> GetProcessMetricsAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves completion statistics (processed, failed, enqueued, ...) for all Sidekiq jobs.</summary>
    Task<JsonElement> GetJobStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists every Sidekiq metric - queue, process and job completion metrics combined.</summary>
    Task<JsonElement> GetCompoundMetricsAsync(CancellationToken cancellationToken = default);
}