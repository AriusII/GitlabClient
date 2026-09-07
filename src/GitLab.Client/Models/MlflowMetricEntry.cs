namespace GitLab.Client.Models;

/// <summary>
///     One metric sample in a <see cref="LogMlflowBatchRequest" />. Kept as its own record rather than
///     flattened into the request because MLflow's <c>log-batch</c> takes an array of these, each with
///     its own timestamp and training step.
/// </summary>
public sealed record MlflowMetricEntry
{
    /// <summary>The metric name.</summary>
    public required string Key { get; init; }

    /// <summary>The recorded value.</summary>
    public required double Value { get; init; }

    /// <summary>When the value was recorded, as a Unix timestamp in milliseconds.</summary>
    public required long Timestamp { get; init; }

    /// <summary>The training step the value belongs to. Optional; GitLab defaults it.</summary>
    public long? Step { get; init; }
}