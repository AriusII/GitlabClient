namespace GitLab.Client.Models;

/// <summary>
///     One recorded value of an MLflow metric on a run (a GitLab candidate), as returned inside
///     <see cref="GitLabMlflowRunData.Metrics" /> and <see cref="GitLabMlflowMetricHistory.Metrics" />.
/// </summary>
public sealed record GitLabMlflowMetric
{
    /// <summary>The metric name - <c>rmse</c>, <c>accuracy</c>.</summary>
    public required string Key { get; init; }

    /// <summary>The recorded value.</summary>
    public double? Value { get; init; }

    /// <summary>
    ///     When the value was recorded, as a Unix timestamp in <em>milliseconds</em>. Deliberately not a
    ///     <see cref="DateTimeOffset" />: MLflow's wire format is a bare integer, and the rest of this
    ///     library reserves <see cref="DateTimeOffset" /> for GitLab's ISO 8601 fields.
    /// </summary>
    public long? Timestamp { get; init; }

    /// <summary>The training step the value belongs to, when the client logged one.</summary>
    public long? Step { get; init; }
}