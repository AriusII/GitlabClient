namespace GitLab.Client.Models;

/// <summary>
///     A bridge (trigger) job in a pipeline - the wire's <c>APIEntitiesCiBridge</c> projection returned
///     by <c>GET /projects/:id/pipelines/:pipeline_id/trigger_jobs</c>.
/// </summary>
/// <remarks>
///     A bridge looks like a job but is a different API schema: it carries a typed downstream pipeline and
///     does not declare the artifact or runner projections of <see cref="GitLabJob" />. Keeping it separate
///     prevents consumers from treating absent job-only data as part of the trigger-jobs contract.
/// </remarks>
public sealed record GitLabBridge
{
    public required long Id { get; init; }

    public required string Status { get; init; }

    public string? Stage { get; init; }

    public required string Name { get; init; }

    public string? Ref { get; init; }

    public bool? Tag { get; init; }

    /// <summary>Test coverage percentage, when the bridge has one.</summary>
    public double? Coverage { get; init; }

    /// <summary>Whether a failure of this bridge still lets the parent pipeline succeed.</summary>
    public bool? AllowFailure { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? FinishedAt { get; init; }

    public DateTimeOffset? ErasedAt { get; init; }

    /// <summary>Run time in seconds. A float on the wire, not an integer.</summary>
    public double? Duration { get; init; }

    /// <summary>Seconds spent queued before the bridge began to run.</summary>
    public double? QueuedDuration { get; init; }

    public GitLabUser? User { get; init; }

    public GitLabCommit? Commit { get; init; }

    public GitLabPipeline? Pipeline { get; init; }

    public string? FailureReason { get; init; }

    public required Uri WebUrl { get; init; }

    public GitLabJobProject? Project { get; init; }

    /// <summary>The pipeline this bridge started in another project, if creation succeeded.</summary>
    public GitLabPipeline? DownstreamPipeline { get; init; }
}