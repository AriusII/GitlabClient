namespace GitLab.Client.Models;

/// <summary>
///     The body of the issue statistics endpoints (<c>GET /issues_statistics</c>,
///     <c>GET /groups/:id/issues_statistics</c>, <c>GET /projects/:id/issues_statistics</c>).
///     <para>
///         The vendored spec declares no schema for these operations, so the shape here is modelled on what
///         GitLab actually returns - a single <c>statistics.counts</c> object. Every member is nullable so a
///         reshaped payload degrades to nulls instead of throwing.
///     </para>
/// </summary>
public sealed record GitLabIssueStatistics
{
    public GitLabIssueStatisticsSummary? Statistics { get; init; }
}