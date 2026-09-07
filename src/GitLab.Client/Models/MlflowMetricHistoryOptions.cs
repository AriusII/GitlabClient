using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     The optional half of <c>GET /projects/:id/ml/mlflow/api/2.0/mlflow/metrics/get-history</c>. The
///     run and the metric name are required and are passed as method arguments instead.
/// </summary>
[GitLabQuery]
public sealed record MlflowMetricHistoryOptions
{
    /// <summary>Page size. GitLab defaults to 1000.</summary>
    public int? MaxResults { get; init; }

    /// <summary>
    ///     The <c>next_page_token</c> from the previous <see cref="GitLabMlflowMetricHistory" />. MLflow
    ///     paginates this endpoint through the body rather than through GitLab's <c>Link</c> header.
    /// </summary>
    public string? PageToken { get; init; }
}