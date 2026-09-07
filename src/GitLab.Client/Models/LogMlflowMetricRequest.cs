namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/runs/log-metric</c>.</summary>
public sealed record LogMlflowMetricRequest
{
    /// <summary>The candidate's UUID.</summary>
    public required string RunId { get; init; }

    /// <summary>The metric name.</summary>
    public required string Key { get; init; }

    /// <summary>The recorded value.</summary>
    public required double Value { get; init; }

    /// <summary>When the value was recorded, as a Unix timestamp in milliseconds. GitLab requires it.</summary>
    public required long Timestamp { get; init; }

    /// <summary>The training step the value belongs to.</summary>
    public long? Step { get; init; }
}