namespace GitLab.Client.Models;

/// <summary>
///     The response of <c>GET /projects/:id/statistics</c>: the project's repository traffic over the
///     last 30 days.
/// </summary>
public sealed record GitLabProjectDailyStatistics
{
    /// <summary>Repository fetch counts - the total, and the per-day breakdown.</summary>
    public GitLabProjectFetchStatistics? Fetches { get; init; }
}