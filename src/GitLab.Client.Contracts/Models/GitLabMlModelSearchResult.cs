namespace GitLab.Client.Models;

/// <summary>
///     One page of <c>GET /projects/:id/ml/mlflow/api/2.0/mlflow/registered-models/search</c>.
/// </summary>
/// <remarks>
///     MLflow paginates this endpoint with an opaque continuation token rather than with GitLab's usual
///     RFC 5988 <c>Link</c> header, which is why the search is not an <c>IAsyncEnumerable</c>: feed
///     <see cref="NextPageToken" /> back as <see cref="MlModelSearchOptions.PageToken" /> to walk the
///     result set, and stop once it comes back empty.
/// </remarks>
public sealed record GitLabMlModelSearchResult
{
    /// <summary>The models on this page. Absent when the filter matched nothing.</summary>
    public IReadOnlyList<GitLabMlModel>? RegisteredModels { get; init; }

    /// <summary>The token that fetches the next page, or null once the last page has been returned.</summary>
    public string? NextPageToken { get; init; }
}