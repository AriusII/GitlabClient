namespace GitLab.Client.Models;

/// <summary>A board column response (<c>APIEntitiesList</c>).</summary>
public sealed record GitLabBoardList
{
    public long? Id { get; init; }

    public GitLabBasicLabel? Label { get; init; }

    public int? Position { get; init; }

    public GitLabMilestone? Milestone { get; init; }

    public GitLabIteration? Iteration { get; init; }

    public GitLabSafeUser? Assignee { get; init; }

    public int? MaxIssueCount { get; init; }

    public int? MaxIssueWeight { get; init; }

    public string? LimitMetric { get; init; }
}