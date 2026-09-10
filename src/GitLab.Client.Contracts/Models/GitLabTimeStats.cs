namespace GitLab.Client.Models;

/// <summary>
///     The time-tracking totals GitLab keeps for an issuable (<c>APIEntitiesIssuableTimeStats</c>),
///     returned by the merge request time-tracking endpoints
///     (<c>time_estimate</c>, <c>reset_time_estimate</c>, <c>add_spent_time</c>,
///     <c>reset_spent_time</c> and <c>time_stats</c>).
/// </summary>
public sealed record GitLabTimeStats
{
    /// <summary>The estimate in seconds. Zero when no estimate has been set.</summary>
    public long TimeEstimate { get; init; }

    /// <summary>The total time logged against the issuable, in seconds.</summary>
    public long TotalTimeSpent { get; init; }

    /// <summary>The estimate rendered the way GitLab writes durations ("3h 30m"), or null when unset.</summary>
    public string? HumanTimeEstimate { get; init; }

    /// <summary>The spent total rendered the way GitLab writes durations ("1d 2h"), or null when unset.</summary>
    public string? HumanTotalTimeSpent { get; init; }
}