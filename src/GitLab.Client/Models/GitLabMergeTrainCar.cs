namespace GitLab.Client.Models;

/// <summary>
///     One car of a merge train (<c>/projects/:id/merge_trains</c>) - that is, one merge request queued
///     on one target branch, not the train as a whole. GitLab's spec calls this entity
///     <c>MergeTrainsCar</c>, and every merge train endpoint returns these, so a project with three
///     queued merge requests answers with three cars.
/// </summary>
public sealed record GitLabMergeTrainCar
{
    public required long Id { get; init; }

    /// <summary>The queued merge request, in GitLab's abbreviated embedding.</summary>
    public GitLabMergeTrainMergeRequest? MergeRequest { get; init; }

    /// <summary>Who added the merge request to the train.</summary>
    public GitLabUser? User { get; init; }

    /// <summary>The merged-result pipeline GitLab created for this car, once it has one.</summary>
    public GitLabPipeline? Pipeline { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>The branch whose train this car is on. One train exists per target branch.</summary>
    public string? TargetBranch { get; init; }

    /// <summary>
    ///     Where the car is in the train's lifecycle. GitLab's vocabulary is <c>idle</c> (queued, no
    ///     pipeline yet), <c>stale</c> (its pipeline no longer reflects the cars ahead of it),
    ///     <c>fresh</c> (pipeline running against the current train), <c>merging</c>, <c>merged</c>, and
    ///     <c>skip_merged</c> (merged without waiting for the pipeline). Modelled as a string rather than an
    ///     enum so a status GitLab adds later cannot turn a response into a deserialization failure.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>When the car actually merged. Null until <see cref="Status" /> reaches a merged state.</summary>
    public DateTimeOffset? MergedAt { get; init; }

    /// <summary>Seconds the car spent on the train, from queueing to merge.</summary>
    public long? Duration { get; init; }
}