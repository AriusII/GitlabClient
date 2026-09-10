namespace GitLab.Client.Models;

/// <summary>Root namespace storage totals embedded in a detailed group response.</summary>
public sealed record GitLabGroupRootStorageStatistics
{
    public long? BuildArtifactsSize { get; init; }

    public long? ContainerRegistrySize { get; init; }

    public bool? ContainerRegistrySizeIsEstimated { get; init; }

    public long? DependencyProxySize { get; init; }

    public long? LfsObjectsSize { get; init; }

    public long? PackagesSize { get; init; }

    public long? PipelineArtifactsSize { get; init; }

    public long? RepositorySize { get; init; }

    public long? SnippetsSize { get; init; }

    public long? StorageSize { get; init; }

    public long? UploadsSize { get; init; }

    public long? WikiSize { get; init; }
}