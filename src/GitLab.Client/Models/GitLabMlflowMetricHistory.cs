namespace GitLab.Client.Models;

/// <summary>One page of a metric's recorded history for a run, as returned by <c>metrics/get-history</c>.</summary>
/// <remarks>
///     Paginated by MLflow's own <see cref="NextPageToken" /> rather than by GitLab's RFC 5988
///     <c>Link</c> header, so it cannot be streamed through the library's normal paging; feed
///     <see cref="NextPageToken" /> back as <c>MlflowMetricHistoryOptions.PageToken</c> to walk it.
/// </remarks>
public sealed record GitLabMlflowMetricHistory
{
    /// <summary>The recorded values in this page.</summary>
    public IReadOnlyList<GitLabMlflowMetric>? Metrics { get; init; }

    /// <summary>The token for the next page, or <see langword="null" /> once the history is exhausted.</summary>
    public string? NextPageToken { get; init; }
}