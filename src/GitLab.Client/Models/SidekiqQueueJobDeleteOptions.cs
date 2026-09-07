using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Metadata filters for <c>DELETE /admin/sidekiq/queues/:queue_name</c>. Every member matches one
///     key of a Sidekiq job's metadata hash; GitLab deletes only jobs whose metadata matches every
///     filter that is set. At least one filter must be set, or GitLab refuses to drain the whole queue.
/// </summary>
[GitLabQuery]
public sealed record SidekiqQueueJobDeleteOptions
{
    public string? OrganizationId { get; init; }

    public string? User { get; init; }

    public string? UserId { get; init; }

    public string? GlUserId { get; init; }

    public string? ScopedUser { get; init; }

    public string? ScopedUserId { get; init; }

    public string? Project { get; init; }

    public string? RootNamespace { get; init; }

    public string? GlRootNamespaceId { get; init; }

    public string? ClientId { get; init; }

    public string? CallerId { get; init; }

    public string? RemoteIp { get; init; }

    public string? JobId { get; init; }

    public string? PipelineId { get; init; }

    public string? RelatedClass { get; init; }

    public string? FeatureCategory { get; init; }

    public string? ArtifactSize { get; init; }

    public string? ArtifactUsedCdn { get; init; }

    public string? ArtifactsDependenciesSize { get; init; }

    public string? ArtifactsDependenciesCount { get; init; }

    public string? RootCallerId { get; init; }

    public string? MergeActionStatus { get; init; }

    public string? BulkImportEntityId { get; init; }

    public string? SidekiqDestinationShardRedis { get; init; }

    public string? KubernetesAgentId { get; init; }

    public string? MvccManifest { get; init; }

    public string? SubscriptionPlan { get; init; }

    public string? AiResource { get; init; }

    public string? WorkerClass { get; init; }
}