namespace GitLab.Client.Models.Requests;

/// <summary>
///     The body of <c>PUT /projects/:id/merge_requests/:merge_request_iid</c>. Every property is optional:
///     what is left null is omitted from the payload and therefore left untouched server-side.
/// </summary>
public sealed record UpdateMergeRequestRequest
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? TargetBranch { get; init; }

    /// <summary>Closes or reopens the merge request.</summary>
    public MergeRequestStateEvent? StateEvent { get; init; }

    public bool? DiscussionLocked { get; init; }

    /// <summary>The single assignee; 0 unassigns. Prefer <see cref="AssigneeIds" />.</summary>
    public long? AssigneeId { get; init; }

    /// <summary>The full assignee set, replacing whatever is there. An empty list unassigns everyone.</summary>
    public IReadOnlyList<long>? AssigneeIds { get; init; }

    /// <summary>The full reviewer set, replacing whatever is there.</summary>
    public IReadOnlyList<long>? ReviewerIds { get; init; }

    /// <summary>
    ///     Replaces the label set. Mutually exclusive with <see cref="AddLabels" /> and
    ///     <see cref="RemoveLabels" />.
    /// </summary>
    public IReadOnlyList<string>? Labels { get; init; }

    public IReadOnlyList<string>? AddLabels { get; init; }

    public IReadOnlyList<string>? RemoveLabels { get; init; }

    /// <summary>The milestone's id; 0 removes the milestone.</summary>
    public long? MilestoneId { get; init; }

    /// <summary>The milestone's title, as an alternative to <see cref="MilestoneId" />.</summary>
    public string? Milestone { get; init; }

    public bool? RemoveSourceBranch { get; init; }

    public bool? AllowCollaboration { get; init; }

    /// <summary>GitLab's older spelling of <see cref="AllowCollaboration" />; the two set the same flag.</summary>
    public bool? AllowMaintainerToPush { get; init; }

    public bool? Squash { get; init; }

    /// <summary>The earliest time the merge request may be merged, for a scheduled merge.</summary>
    public DateTimeOffset? MergeAfter { get; init; }

    public int? ApprovalsBeforeMerge { get; init; }
}