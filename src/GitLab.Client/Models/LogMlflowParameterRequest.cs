namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/runs/log-parameter</c>.</summary>
public sealed record LogMlflowParameterRequest
{
    /// <summary>The candidate's UUID.</summary>
    public required string RunId { get; init; }

    /// <summary>The parameter name.</summary>
    public required string Key { get; init; }

    /// <summary>The parameter value. MLflow transports every parameter as a string.</summary>
    public required string Value { get; init; }
}