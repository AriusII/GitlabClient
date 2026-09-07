using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class SidekiqRepository(IGitLabApiConnection connection) : ISidekiqRepository
{
    public Task DeleteQueueJobsAsync(string queueName, SidekiqQueueJobDeleteOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("admin").Literal("sidekiq").Literal("queues").Escaped(queueName)
                .QueryFrom(options).Build(),
            cancellationToken);
    }

    public Task<JsonElement> GetQueueMetricsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("sidekiq").Literal("queue_metrics").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetProcessMetricsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("sidekiq").Literal("process_metrics").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetJobStatsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("sidekiq").Literal("job_stats").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetCompoundMetricsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("sidekiq").Literal("compound_metrics").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }
}