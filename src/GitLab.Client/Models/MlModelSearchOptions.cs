using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for <c>GET /projects/:id/ml/mlflow/api/2.0/mlflow/registered-models/search</c>.
/// </summary>
[GitLabQuery]
public readonly record struct MlModelSearchOptions
{
    /// <summary>
    ///     The MLflow search filter. It must be written as <c>name='value'</c>; GitLab supports filtering
    ///     by name and by nothing else.
    /// </summary>
    public string? Filter { get; init; }

    /// <summary>Maximum number of models to return. GitLab defaults to 200 and caps the value at 1000.</summary>
    public int? MaxResults { get; init; }

    /// <summary>
    ///     Order criteria - <c>name</c> or <c>last_updated_timestamp</c>, optionally suffixed with
    ///     <c>ASC</c> (the default) or <c>DESC</c>. GitLab defaults to <c>name ASC</c>. Ordering by model
    ///     metadata is not supported.
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>
    ///     The continuation token from the previous page's
    ///     <see cref="GitLabMlModelSearchResult.NextPageToken" />. Null asks for the first page.
    /// </summary>
    public string? PageToken { get; init; }
}