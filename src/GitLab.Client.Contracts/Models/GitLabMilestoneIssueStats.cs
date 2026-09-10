namespace GitLab.Client.Models;

/// <summary>
///     The issue counts GitLab includes in the <c>issue_stats</c> projection of a release milestone
///     (<c>APIEntitiesMilestoneWithStats</c>).
/// </summary>
public sealed record GitLabMilestoneIssueStats
{
    /// <summary>The number of issues associated with the milestone.</summary>
    public int? Total { get; init; }

    /// <summary>The number of associated issues that are closed.</summary>
    public int? Closed { get; init; }
}