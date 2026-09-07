namespace GitLab.Client.Models;

/// <summary>
///     Storage usage for a group, in bytes. Returned only when the request asked for it
///     (<c>statistics=true</c>) and only to users who may see it, so every member is nullable.
/// </summary>
public sealed record GitLabGroupStatistics
{
    public long? StorageSize { get; init; }

    public long? RepositorySize { get; init; }

    public long? WikiSize { get; init; }

    public long? LfsObjectsSize { get; init; }

    public long? JobArtifactsSize { get; init; }

    public long? PipelineArtifactsSize { get; init; }

    public long? PackagesSize { get; init; }

    public long? SnippetsSize { get; init; }

    public long? UploadsSize { get; init; }
}