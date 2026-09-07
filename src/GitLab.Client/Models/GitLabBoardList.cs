namespace GitLab.Client.Models;

/// <summary>
///     A single column of an issue board (<c>/projects/:id/boards/:board_id/lists</c>). Exactly one of
///     <see cref="Label" />, <see cref="Milestone" /> or <see cref="Iteration" /> is populated, according
///     to what the list scopes on.
/// </summary>
/// <remarks>
///     The board list's <c>assignee</c> field is deliberately not modelled. GitLab projects it as
///     <c>APIEntitiesUserSafe</c> (id, username, public_email, name), which has no <c>web_url</c> - a
///     member <see cref="GitLabUser" /> marks required. Adding assignee lists later means introducing a
///     separate reduced user DTO, not loosening <see cref="GitLabUser" />.
/// </remarks>
public sealed record GitLabBoardList
{
    public required long Id { get; init; }

    public GitLabLabel? Label { get; init; }

    /// <summary>One-based position of this list among the board's lists.</summary>
    public int? Position { get; init; }

    public GitLabMilestone? Milestone { get; init; }

    public GitLabIteration? Iteration { get; init; }

    public int? MaxIssueCount { get; init; }

    public int? MaxIssueWeight { get; init; }

    /// <summary>Which work-in-progress limit the list enforces - "all_metrics", "issue_count" or "issue_weights".</summary>
    public string? LimitMetric { get; init; }
}