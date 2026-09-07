namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/issues/:issue_iid</c>. Every member is optional and omitted
///     when null, so this is a partial update: send only what should change.
///     <para>
///         Closing and reopening go through <see cref="StateEvent" /> rather than a <c>state</c> field,
///         because that is how GitLab models the transition.
///     </para>
/// </summary>
public sealed record UpdateIssueRequest
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    /// <summary>Closes or reopens the issue. See <see cref="GitLabIssueStateEvent" />.</summary>
    public GitLabIssueStateEvent? StateEvent { get; init; }

    /// <summary>Backdates the update. Accepted only from administrators and project owners.</summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <inheritdoc cref="CreateIssueRequest.CreatedAt" />
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Replaces the assignees. An empty list unassigns everyone.</summary>
    public IReadOnlyList<long>? AssigneeIds { get; init; }

    /// <inheritdoc cref="CreateIssueRequest.MilestoneId" />
    public long? MilestoneId { get; init; }

    /// <inheritdoc cref="CreateIssueRequest.Milestone" />
    public string? Milestone { get; init; }

    /// <summary>
    ///     Replaces the label set outright. Use <see cref="AddLabels" /> and <see cref="RemoveLabels" /> to
    ///     change it incrementally.
    /// </summary>
    public IReadOnlyList<string>? Labels { get; init; }

    public IReadOnlyList<string>? AddLabels { get; init; }

    public IReadOnlyList<string>? RemoveLabels { get; init; }

    public DateOnly? DueDate { get; init; }

    public DateOnly? StartDate { get; init; }

    public bool? Confidential { get; init; }

    public bool? DiscussionLocked { get; init; }

    public GitLabIssueType? IssueType { get; init; }

    /// <summary>Only applied to incidents.</summary>
    public GitLabIssueSeverity? Severity { get; init; }

    public int? Weight { get; init; }
}