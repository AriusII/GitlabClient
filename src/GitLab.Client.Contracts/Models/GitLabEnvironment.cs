namespace GitLab.Client.Models;

/// <summary>A deployment-environment response (<c>APIEntitiesEnvironment</c>).</summary>
public sealed record GitLabEnvironment
{
    public long? Id { get; init; }

    public string? Name { get; init; }

    public string? Slug { get; init; }

    public Uri? ExternalUrl { get; init; }

    public string? State { get; init; }

    public string? Tier { get; init; }

    public DateTimeOffset? AutoStopAt { get; init; }

    public string? AutoStopSetting { get; init; }

    public string? Description { get; init; }

    public string? KubernetesNamespace { get; init; }

    public string? FluxResourcePath { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public GitLabBasicProjectDetails? Project { get; init; }

    public GitLabDeployment? LastDeployment { get; init; }

    public GitLabClusterAgent? ClusterAgent { get; init; }
}