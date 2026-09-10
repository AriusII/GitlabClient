namespace GitLab.Client.Models;

/// <summary>
///     A GitLab milestone. The same entity serves both scopes (<c>/projects/{id}/milestones</c> and
///     <c>/groups/{id}/milestones</c>); exactly one of <see cref="ProjectId" /> and <see cref="GroupId" />
///     is filled in, depending on which owns the milestone.
/// </summary>
public sealed record GitLabMilestone
{
    public required long Id { get; init; }

    public required long Iid { get; init; }

    /// <summary>Set on a project milestone; <c>null</c> on a group milestone.</summary>
    public long? ProjectId { get; init; }

    /// <summary>
    ///     Set on a group milestone; <c>null</c> on a project milestone. Typed as a number even though the
    ///     spec declares it a string - GitLab sends the numeric group id, and the context's
    ///     <c>AllowReadingFromString</c> covers the quoted form either way.
    /// </summary>
    public long? GroupId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required string State { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public DateOnly? DueDate { get; init; }

    public DateOnly? StartDate { get; init; }

    public bool? Expired { get; init; }

    public required Uri WebUrl { get; init; }

    /// <summary>
    ///     Issue counts returned when this milestone is embedded in a release. The standalone Milestones
    ///     endpoints do not guarantee this projection.
    /// </summary>
    public GitLabMilestoneIssueStats? IssueStats { get; init; }
}