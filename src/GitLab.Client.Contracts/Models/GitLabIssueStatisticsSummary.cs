namespace GitLab.Client.Models;

/// <summary>
///     The <c>statistics</c> node of an <c>issues_statistics</c> response. It exists only to carry
///     <see cref="Counts" />; GitLab nests it this way to leave room for further breakdowns.
/// </summary>
public sealed record GitLabIssueStatisticsSummary
{
    public GitLabIssueCounts? Counts { get; init; }
}