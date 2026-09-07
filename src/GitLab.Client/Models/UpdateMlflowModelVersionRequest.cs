namespace GitLab.Client.Models;

/// <summary>Body of <c>PATCH /projects/:id/ml/mlflow/api/2.0/mlflow/model-versions/update</c>.</summary>
/// <remarks>
///     The only <c>PATCH</c> in the MLflow surface, and the only mutation that identifies its target
///     entirely through the body rather than through the route.
/// </remarks>
public sealed record UpdateMlflowModelVersionRequest
{
    /// <summary>The model version's name.</summary>
    public string? Name { get; init; }

    /// <summary>The new description.</summary>
    public string? Description { get; init; }
}