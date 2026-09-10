namespace GitLab.Client.Models.Requests;

/// <summary>
///     The body of <c>POST /projects/:id/merge_requests</c>. Only <see cref="Title" />,
///     <see cref="SourceBranch" /> and <see cref="TargetBranch" /> are mandatory; every other member is
///     omitted when null, letting GitLab apply its project defaults.
/// </summary>
public sealed record CreateMergeRequestRequest
{
    public required string Title { get; init; }

    public required string SourceBranch { get; init; }

    public required string TargetBranch { get; init; }

    /// <summary>
    ///     The target project's database id for a cross-project merge request. When omitted, GitLab targets
    ///     the project identified by the route.
    /// </summary>
    public long? TargetProjectId { get; init; }

    /// <summary>
    ///     The legacy single-assignee id. Prefer <see cref="AssigneeIds" /> for GitLab's multi-assignee
    ///     model.
    /// </summary>
    public long? AssigneeId { get; init; }

    /// <summary>The complete initial assignee set. An empty list creates the merge request unassigned.</summary>
    public IReadOnlyList<long>? AssigneeIds { get; init; }

    /// <summary>The complete initial reviewer set.</summary>
    public IReadOnlyList<long>? ReviewerIds { get; init; }

    public string? Description { get; init; }

    /// <summary>
    ///     Replaces the initial label set. It is mutually exclusive with <see cref="AddLabels" /> and
    ///     <see cref="RemoveLabels" />.
    /// </summary>
    public IReadOnlyList<string>? Labels { get; init; }

    /// <summary>Labels to add to the merge request's initial label set.</summary>
    public IReadOnlyList<string>? AddLabels { get; init; }

    /// <summary>Labels to remove from the merge request's initial label set.</summary>
    public IReadOnlyList<string>? RemoveLabels { get; init; }

    /// <summary>The milestone database id.</summary>
    public long? MilestoneId { get; init; }

    /// <summary>The milestone title, as an alternative to <see cref="MilestoneId" />.</summary>
    public string? Milestone { get; init; }

    /// <summary>Deletes the source branch automatically after the merge succeeds.</summary>
    public bool? RemoveSourceBranch { get; init; }

    /// <summary>Lets users who can merge into the target project push to the source branch.</summary>
    public bool? AllowCollaboration { get; init; }

    /// <summary>
    ///     GitLab's legacy spelling for <see cref="AllowCollaboration" />. Both fields address the same
    ///     server-side setting; use <see cref="AllowCollaboration" /> in new code.
    /// </summary>
    public bool? AllowMaintainerToPush { get; init; }

    /// <summary>Requests squashing of source commits when this merge request is merged.</summary>
    public bool? Squash { get; init; }

    /// <summary>The earliest instant at which GitLab may merge the merge request.</summary>
    public DateTimeOffset? MergeAfter { get; init; }

    /// <summary>The project approval count to require before merging, where the GitLab tier permits it.</summary>
    public int? ApprovalsBeforeMerge { get; init; }
}