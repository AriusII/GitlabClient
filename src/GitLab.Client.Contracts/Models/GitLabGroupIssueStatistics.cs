namespace GitLab.Client.Models;

/// <summary>
///     What <c>GET /groups/:id/issues_statistics</c> answers with. GitLab wraps the numbers twice, in a
///     <c>statistics</c> object holding a <c>counts</c> object, and this type mirrors that shape rather
///     than flattening it - a flattened DTO would not deserialize.
/// </summary>
public sealed record GitLabGroupIssueStatistics
{
    public GitLabGroupIssueStatisticsBreakdown? Statistics { get; init; }
}