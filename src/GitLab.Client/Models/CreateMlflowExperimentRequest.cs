namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/experiments/create</c>.</summary>
public sealed record CreateMlflowExperimentRequest
{
    /// <summary>The experiment name. Must be unique within the project.</summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Tags to store against the experiment. The spec leaves the element type open; MLflow sends
    ///     <c>{ "key": ..., "value": ... }</c> pairs, which is what this maps to.
    /// </summary>
    public IReadOnlyList<GitLabMlflowKeyValue>? Tags { get; init; }

    /// <summary>
    ///     MLflow's artifact location. GitLab accepts and ignores it - it manages artifact storage itself -
    ///     so it exists here only so an MLflow client's payload round-trips unchanged.
    /// </summary>
    public string? ArtifactLocation { get; init; }
}