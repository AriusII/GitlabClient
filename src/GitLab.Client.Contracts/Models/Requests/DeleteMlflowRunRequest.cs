namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/runs/delete</c>.</summary>
/// <remarks>
///     MLflow deletes through a POST with a body rather than through <c>DELETE</c>, which is why this
///     type exists at all for a single-field payload.
/// </remarks>
public sealed record DeleteMlflowRunRequest
{
    /// <summary>The candidate's UUID.</summary>
    public required string RunId { get; init; }
}