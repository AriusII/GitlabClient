namespace GitLab.Client.Models;

/// <summary>The <c>fetches</c> object of <see cref="GitLabProjectDailyStatistics" />.</summary>
/// <remarks>
///     The spec types <c>fetches</c> as a bare untyped <c>object</c>, so this shape comes from the
///     endpoint's documentation. Both members are optional for that reason.
/// </remarks>
public sealed record GitLabProjectFetchStatistics
{
    /// <summary>Fetches across the whole window.</summary>
    public long? Total { get; init; }

    /// <summary>The per-day breakdown, most recent day first.</summary>
    public IReadOnlyList<GitLabProjectDailyFetchCount>? Days { get; init; }
}