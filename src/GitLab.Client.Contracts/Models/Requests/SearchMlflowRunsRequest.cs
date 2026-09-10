namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/runs/search</c>.</summary>
public sealed record SearchMlflowRunsRequest
{
    /// <summary>
    ///     The experiments to search, relative to the project. GitLab supports exactly one element here -
    ///     the array is MLflow's shape, not a capability.
    /// </summary>
    public required IReadOnlyList<string> ExperimentIds { get; init; }

    /// <summary>Page size. GitLab defaults to 200 and caps it at 1000.</summary>
    public int? MaxResults { get; init; }

    /// <summary>
    ///     Order criteria: a column of the candidate (<c>created_at</c>, <c>name</c>), optionally with a
    ///     direction, or a metric prefixed with <c>metrics.</c> (<c>metrics.my_metric DESC</c>). Ordering by
    ///     a candidate parameter or by metadata is not supported. GitLab defaults to <c>created_at DESC</c>.
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>The <c>page_token</c> from the previous page.</summary>
    public string? PageToken { get; init; }
}