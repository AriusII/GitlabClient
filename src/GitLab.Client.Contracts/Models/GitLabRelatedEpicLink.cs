namespace GitLab.Client.Models;

/// <summary>A two-way link between two group epics.</summary>
public sealed record GitLabRelatedEpicLink
{
    public long? Id { get; init; }

    public GitLabEpic? SourceEpic { get; init; }

    public GitLabEpic? TargetEpic { get; init; }

    /// <summary><c>relates_to</c>, <c>blocks</c>, or <c>is_blocked_by</c>.</summary>
    public string? LinkType { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}