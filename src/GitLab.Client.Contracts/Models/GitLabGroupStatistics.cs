namespace GitLab.Client.Models;

/// <summary>
///     The inline <c>statistics</c> object of a group response. GitLab returns each size as a numeric
///     byte count. The source-generated JSON context also accepts legacy string-encoded numbers.
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