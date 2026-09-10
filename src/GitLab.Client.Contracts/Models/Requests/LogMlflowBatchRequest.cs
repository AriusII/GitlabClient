namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/runs/log-batch</c> - many metrics and
///     parameters in one call, which is how an MLflow client flushes a training loop.
/// </summary>
public sealed record LogMlflowBatchRequest
{
    /// <summary>The candidate's UUID.</summary>
    public required string RunId { get; init; }

    /// <summary>The metric samples to record. GitLab defaults this to an empty array.</summary>
    public IReadOnlyList<MlflowMetricEntry>? Metrics { get; init; }

    /// <summary>The parameters to record. GitLab defaults this to an empty array.</summary>
    public IReadOnlyList<MlflowParameterEntry>? Params { get; init; }
}