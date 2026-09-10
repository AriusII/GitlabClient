namespace GitLab.Client.Models;

/// <summary>The <c>statistics</c> member of <see cref="GitLabGroupIssueStatistics" />.</summary>
public sealed record GitLabGroupIssueStatisticsBreakdown
{
    public GitLabGroupIssueCounts? Counts { get; init; }
}