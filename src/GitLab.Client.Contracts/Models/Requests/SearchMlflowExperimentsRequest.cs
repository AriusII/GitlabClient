namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /projects/:id/ml/mlflow/api/2.0/mlflow/experiments/search</c>.</summary>
public sealed record SearchMlflowExperimentsRequest
{
    /// <summary>Page size. GitLab defaults to 200 and caps it at 1000.</summary>
    public int? MaxResults { get; init; }

    /// <summary>
    ///     Order criteria over a column of the experiment - <c>created_at</c> or <c>name</c>, optionally
    ///     with a direction (<c>created_at DESC</c>). GitLab defaults to <c>created_at DESC</c>.
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>The <c>page_token</c> from the previous page.</summary>
    public string? PageToken { get; init; }

    /// <summary>MLflow's filter expression. GitLab accepts and ignores it.</summary>
    public string? Filter { get; init; }
}