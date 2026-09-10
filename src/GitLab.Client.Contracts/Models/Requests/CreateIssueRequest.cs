namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/issues</c>. Only <see cref="Title" /> is required; every other
///     member is omitted from the payload when left null, so an unset property means "let GitLab decide"
///     rather than "clear this".
/// </summary>
public sealed record CreateIssueRequest
{
    public required string Title { get; init; }

    public string? Description { get; init; }

    /// <summary>Backdates the issue. Accepted only from administrators and project owners.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Pins the issue to a specific IID. Accepted only from administrators and project owners.</summary>
    public long? Iid { get; init; }

    /// <summary>The IID of a merge request whose open discussions this issue should resolve.</summary>
    public long? MergeRequestToResolveDiscussionsOf { get; init; }

    /// <summary>
    ///     One discussion to resolve rather than all of them. Requires
    ///     <see cref="MergeRequestToResolveDiscussionsOf" />.
    /// </summary>
    public string? DiscussionToResolve { get; init; }

    public IReadOnlyList<long>? AssigneeIds { get; init; }

    /// <summary>The milestone id to assign. Mutually exclusive with <see cref="Milestone" />.</summary>
    public long? MilestoneId { get; init; }

    /// <summary>
    ///     The title of a project or ancestor-group milestone to assign. Mutually exclusive with
    ///     <see cref="MilestoneId" />.
    /// </summary>
    public string? Milestone { get; init; }

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

    /// <summary>
    ///     The database ID of the epic to associate with the issue. GitLab 19.x keeps the older
    ///     <c>epic_iid</c> parameter for compatibility, but it is deprecated and intentionally not exposed
    ///     by this request contract.
    /// </summary>
    public long? EpicId { get; init; }
}