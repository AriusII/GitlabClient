namespace GitLab.Client.Models;

/// <summary>Repository and storage counters embedded in a project response.</summary>
public sealed record GitLabProjectStatistics
{
    public int? CommitCount { get; init; }

    public long? StorageSize { get; init; }

    public long? RepositorySize { get; init; }

    public long? WikiSize { get; init; }

    public long? LfsObjectsSize { get; init; }

    public long? JobArtifactsSize { get; init; }

    public long? PipelineArtifactsSize { get; init; }

    public long? PackagesSize { get; init; }

    public long? SnippetsSize { get; init; }

    public long? UploadsSize { get; init; }

    public long? ContainerRegistrySize { get; init; }
}