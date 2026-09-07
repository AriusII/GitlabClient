namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/experiments/delete</c>.</summary>
/// <remarks>
///     MLflow deletes through a POST with a body rather than through <c>DELETE</c>, which is why this
///     type exists at all for a single-field payload.
/// </remarks>
public sealed record DeleteMlflowExperimentRequest
{
    /// <summary>The experiment ID, relative to the project.</summary>
    public required string ExperimentId { get; init; }
}