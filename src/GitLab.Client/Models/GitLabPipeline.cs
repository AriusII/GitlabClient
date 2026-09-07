namespace GitLab.Client.Models;

/// <summary>A CI/CD pipeline run for a project, as returned by the GitLab Pipelines API.</summary>
public sealed record GitLabPipeline
{
    public required long Id { get; init; }

    public long? Iid { get; init; }

    public long? ProjectId { get; init; }

    public required string Sha { get; init; }

    public required string Ref { get; init; }

    public required string Status { get; init; }

    public string? Source { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public required Uri WebUrl { get; init; }

    /// <summary>
    ///     The pipeline's display name, settable through the metadata endpoint. Part of GitLab's detailed
    ///     pipeline shape only, so it is null on a pipeline embedded in another resource.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>The commit the ref pointed at before the pipeline's push. Detailed shape only.</summary>
    public string? BeforeSha { get; init; }

    /// <summary>Whether <see cref="Ref" /> is a tag rather than a branch. Detailed shape only.</summary>
    public bool? Tag { get; init; }

    /// <summary>Why GitLab refused to run the pipeline's configuration, if it did. Detailed shape only.</summary>
    public string? YamlErrors { get; init; }

    /// <summary>The user the pipeline runs as. Detailed shape only.</summary>
    public GitLabUser? User { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? FinishedAt { get; init; }

    public DateTimeOffset? CommittedAt { get; init; }

    /// <summary>Run time in seconds. Detailed shape only.</summary>
    public double? Duration { get; init; }

    /// <summary>Seconds the pipeline spent queued before it started. Detailed shape only.</summary>
    public double? QueuedDuration { get; init; }

    /// <summary>Test coverage as a percentage, aggregated across the pipeline's jobs. Detailed shape only.</summary>
    public double? Coverage { get; init; }

    /// <summary>Whether the pipeline is old enough that GitLab has archived it. Detailed shape only.</summary>
    public bool? Archived { get; init; }
}