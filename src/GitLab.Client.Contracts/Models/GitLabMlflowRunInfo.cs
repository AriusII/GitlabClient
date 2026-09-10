using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     The identity and lifecycle half of an MLflow run. GitLab maps an MLflow run onto a
///     <em>candidate</em> of an ML experiment, so <see cref="RunId" /> is the candidate's UUID.
/// </summary>
public sealed record GitLabMlflowRunInfo
{
    /// <summary>The candidate's UUID. This is what every <c>run_id</c> parameter takes.</summary>
    public string? RunId { get; init; }

    /// <summary>MLflow's legacy alias for <see cref="RunId" />, echoed with the same value.</summary>
    public string? RunUuid { get; init; }

    /// <summary>The owning experiment's ID, relative to the project.</summary>
    public string? ExperimentId { get; init; }

    /// <summary>When the run started, as a Unix timestamp in milliseconds.</summary>
    public long? StartTime { get; init; }

    /// <summary>When the run finished, as a Unix timestamp in milliseconds.</summary>
    public long? EndTime { get; init; }

    /// <summary>The candidate's display name.</summary>
    public string? RunName { get; init; }

    /// <summary>
    ///     <c>RUNNING</c>, <c>SCHEDULED</c>, <c>FINISHED</c>, <c>FAILED</c> or <c>KILLED</c>. Deliberately a
    ///     bare <see cref="string" />: the spec types this response field as an unconstrained string - only
    ///     the <em>request</em> side of <c>runs/update</c> is enumerated - and a status GitLab adds later
    ///     must not turn a healthy response into a <see cref="System.Text.Json.JsonException" />. See
    ///     <see cref="GitLabMlflowRunStatus" /> for the write side.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    ///     Where this run's artifacts live. GitLab answers in MLflow's own
    ///     <c>mlflow-artifacts:&lt;version&gt;</c> form, and can answer with an empty string, so this stays a
    ///     <see cref="string" /> rather than a <see cref="Uri" /> - the empty case would throw on parse.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "GitLab returns MLflow's opaque 'mlflow-artifacts:<version>' form, and an empty string when the "
            + "run has no artifacts; neither round-trips reliably through Uri.")]
    public string? ArtifactUri { get; init; }

    /// <summary>MLflow's soft-delete marker - <c>active</c> or <c>deleted</c>.</summary>
    public string? LifecycleStage { get; init; }

    /// <summary>The MLflow user ID. GitLab does not populate this from its own user model.</summary>
    public string? UserId { get; init; }
}