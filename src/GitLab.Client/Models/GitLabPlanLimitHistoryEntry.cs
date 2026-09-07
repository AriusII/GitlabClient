namespace GitLab.Client.Models;

/// <summary>
///     One recorded change to a plan limit, as they appear in the arrays nested under
///     <see cref="GitLabPlanLimits.LimitsHistory" />.
/// </summary>
public sealed record GitLabPlanLimitHistoryEntry
{
    /// <summary>Unix timestamp of the change.</summary>
    public long? Timestamp { get; init; }

    /// <summary>ID of the user who made the change.</summary>
    public long? UserId { get; init; }

    /// <summary>Username of the user who made the change.</summary>
    public string? Username { get; init; }

    /// <summary>The limit's new value after this change.</summary>
    public long? Value { get; init; }
}