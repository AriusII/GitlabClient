using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Sidekiq resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ISidekiqService), typeof(ISidekiqClient))]
internal interface ISidekiqRepository
{
    Task DeleteQueueJobsAsync(string queueName, SidekiqQueueJobDeleteOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetQueueMetricsAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> GetProcessMetricsAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> GetJobStatsAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> GetCompoundMetricsAsync(CancellationToken cancellationToken = default);
}