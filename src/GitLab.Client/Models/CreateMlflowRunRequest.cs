namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/runs/create</c>.</summary>
public sealed record CreateMlflowRunRequest
{
    /// <summary>
    ///     The experiment to create the run under, relative to the project.
    ///     <para>
    ///         A <see cref="long" />, not a <see cref="string" />: this is the one operation in the whole
    ///         MLflow surface where the spec types <c>experiment_id</c> as an integer. Every response, and
    ///         every other request, carries it as a string.
    ///     </para>
    /// </summary>
    public required long ExperimentId { get; init; }

    /// <summary>When the run started, as a Unix timestamp in milliseconds. GitLab defaults it to 0.</summary>
    public long? StartTime { get; init; }

    /// <summary>MLflow's user ID. GitLab accepts and ignores it.</summary>
    public string? UserId { get; init; }

    /// <summary>
    ///     Tags to store against the run. GitLab stores them but does not display them. The spec leaves the
    ///     element type open; MLflow sends <c>{ "key": ..., "value": ... }</c> pairs.
    /// </summary>
    public IReadOnlyList<GitLabMlflowKeyValue>? Tags { get; init; }

    /// <summary>A display name for the run.</summary>
    public string? RunName { get; init; }
}