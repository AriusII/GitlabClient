namespace GitLab.Client.Models;

/// <summary>The payload half of an MLflow run: everything the client logged against the candidate.</summary>
public sealed record GitLabMlflowRunData
{
    /// <summary>
    ///     The latest recorded value of each metric. Use <c>metrics/get-history</c> for the full series of
    ///     one metric.
    /// </summary>
    public IReadOnlyList<GitLabMlflowMetric>? Metrics { get; init; }

    /// <summary>The run's parameters (hyper-parameters), as MLflow key/value pairs.</summary>
    public IReadOnlyList<GitLabMlflowKeyValue>? Params { get; init; }

    /// <summary>The run's tags.</summary>
    public IReadOnlyList<GitLabMlflowKeyValue>? Tags { get; init; }
}