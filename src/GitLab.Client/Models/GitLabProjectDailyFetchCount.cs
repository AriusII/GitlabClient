namespace GitLab.Client.Models;

/// <summary>One day of the <c>fetches.days</c> breakdown on <see cref="GitLabProjectDailyStatistics" />.</summary>
public sealed record GitLabProjectDailyFetchCount
{
    /// <summary>Fetches recorded on <see cref="Date" />.</summary>
    public long? Count { get; init; }

    /// <summary>The day being counted.</summary>
    public DateOnly? Date { get; init; }
}