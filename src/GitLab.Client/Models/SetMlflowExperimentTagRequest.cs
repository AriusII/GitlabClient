namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/experiments/set-experiment-tag</c>.</summary>
public sealed record SetMlflowExperimentTagRequest
{
    /// <summary>The experiment ID, relative to the project.</summary>
    public required string ExperimentId { get; init; }

    /// <summary>The tag name.</summary>
    public required string Key { get; init; }

    /// <summary>The tag value.</summary>
    public required string Value { get; init; }
}