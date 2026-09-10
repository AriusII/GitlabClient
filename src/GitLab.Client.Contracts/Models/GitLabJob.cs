namespace GitLab.Client.Models;

/// <summary>
///     A CI/CD job in a project's pipeline, as returned by the GitLab Jobs API.
///     <para>
///         GitLab returns two shapes here: the full <c>APIEntitiesCiJob</c> from list/get/cancel/retry/erase,
///         and a leaner basic shape from <c>play</c> and from <c>GET /runners/:id/jobs</c>. Every member
///         beyond <see cref="WebUrl" /> is therefore nullable and can legitimately be absent - reading
///         <see cref="Artifacts" /> straight after a play call, for instance, gives <see langword="null" />.
///     </para>
/// </summary>
public sealed record GitLabJob
{
    public required long Id { get; init; }

    public required string Status { get; init; }

    public string? Stage { get; init; }

    public required string Name { get; init; }

    public string? Ref { get; init; }

    public bool? Tag { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? FinishedAt { get; init; }

    /// <summary>Run time in seconds. A float on the wire, not an integer.</summary>
    public double? Duration { get; init; }

    public GitLabUser? User { get; init; }

    public required Uri WebUrl { get; init; }

    /// <summary>Whether a failure of this job still lets the pipeline succeed.</summary>
    public bool? AllowFailure { get; init; }

    /// <summary>Test coverage as a percentage, parsed out of the job log by the project's coverage regex.</summary>
    public double? Coverage { get; init; }

    /// <summary>Seconds the job spent queued before a runner picked it up. A float on the wire.</summary>
    public double? QueuedDuration { get; init; }

    /// <summary>When the job's artifacts and log were erased, if they were.</summary>
    public DateTimeOffset? ErasedAt { get; init; }

    /// <summary>
    ///     Why a failed job failed - "script_failure", "runner_system_failure", "job_execution_timeout" and
    ///     the rest of GitLab's list. Null unless <see cref="Status" /> is "failed".
    /// </summary>
    public string? FailureReason { get; init; }

    public DateTimeOffset? ArtifactsExpireAt { get; init; }

    /// <summary>Whether the job is old enough that GitLab has archived it, making it un-retryable.</summary>
    public bool? Archived { get; init; }

    /// <summary>The runner tags the job requires.</summary>
    public IReadOnlyList<string>? TagList { get; init; }

    /// <summary>The commit the job ran against.</summary>
    public GitLabCommit? Commit { get; init; }

    /// <summary>The pipeline the job belongs to.</summary>
    public GitLabPipeline? Pipeline { get; init; }

    /// <summary>The artifacts the job produced. Empty or null once the job has been erased.</summary>
    public IReadOnlyList<GitLabJobArtifact>? Artifacts { get; init; }

    /// <summary>
    ///     The aggregate archive GitLab exposes directly on the job. This is distinct from
    ///     <see cref="Artifacts" />, whose elements include their artifact type and packaging format.
    /// </summary>
    public GitLabJobArtifactFile? ArtifactsFile { get; init; }

    /// <summary>The reduced project projection attached to the full job response.</summary>
    public GitLabJobProject? Project { get; init; }

    /// <summary>The runner which executed this job, when GitLab discloses it.</summary>
    public GitLabRunner? Runner { get; init; }

    /// <summary>The concrete runner-manager process which executed this job, when GitLab discloses it.</summary>
    public GitLabRunnerManager? RunnerManager { get; init; }
}