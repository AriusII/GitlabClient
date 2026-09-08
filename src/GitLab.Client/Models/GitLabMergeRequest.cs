namespace GitLab.Client.Models;

/// <summary>
///     A merge request within a GitLab project, as returned by the Merge Requests API.
///     <para>
///         Several members are populated only by specific endpoints rather than every listing - see
///         <see cref="TimeStats" />, <see cref="ChangesCount" /> and <see cref="Changes" /> for the notable
///         cases. The type is also embedded by other resources, such as
///         <see cref="GitLabStatusCheckResponse.MergeRequest" />.
///     </para>
/// </summary>
public sealed record GitLabMergeRequest
{
    public required long Id { get; init; }

    public required long Iid { get; init; }

    public long? ProjectId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required string State { get; init; }

    public required string SourceBranch { get; init; }

    public required string TargetBranch { get; init; }

    public GitLabUser? Author { get; init; }

    public bool? Draft { get; init; }

    public string? MergeStatus { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public required Uri WebUrl { get; init; }

    /// <summary>
    ///     Whether the authenticated user is subscribed to this merge request. Only returned by the
    ///     Resource subscriptions endpoints; null on the plain list/get responses.
    /// </summary>
    public bool? Subscribed { get; init; }

    /// <summary>
    ///     The finer-grained companion to <see cref="MergeStatus" /> - "mergeable", "draft_status",
    ///     "not_approved", "ci_still_running" and a growing list of others. Left as a string on purpose:
    ///     GitLab types it as a bare string in the spec and adds values as it adds merge checks, and a value
    ///     this library has not seen must not turn a healthy response into a <c>JsonException</c>.
    /// </summary>
    public string? DetailedMergeStatus { get; init; }

    /// <summary>
    ///     The head commit of the source branch. Pass it back to a merge to make the merge safe against a concurrent
    ///     push.
    /// </summary>
    public string? Sha { get; init; }

    public string? MergeCommitSha { get; init; }

    public string? SquashCommitSha { get; init; }

    public long? SourceProjectId { get; init; }

    public long? TargetProjectId { get; init; }

    public IReadOnlyList<string>? Labels { get; init; }

    public IReadOnlyList<GitLabUser>? Assignees { get; init; }

    public IReadOnlyList<GitLabUser>? Reviewers { get; init; }

    /// <summary>Who merged it. GitLab's replacement for the older <c>merged_by</c>.</summary>
    public GitLabUser? MergeUser { get; init; }

    public DateTimeOffset? MergedAt { get; init; }

    public GitLabUser? ClosedBy { get; init; }

    public DateTimeOffset? ClosedAt { get; init; }

    /// <summary>When GitLab finished preparing the merge request after it was created.</summary>
    public DateTimeOffset? PreparedAt { get; init; }

    public bool? MergeWhenPipelineSucceeds { get; init; }

    public bool? HasConflicts { get; init; }

    public bool? BlockingDiscussionsResolved { get; init; }

    public bool? DiscussionLocked { get; init; }

    public bool? Squash { get; init; }

    public bool? ShouldRemoveSourceBranch { get; init; }

    public bool? ForceRemoveSourceBranch { get; init; }

    public bool? AllowCollaboration { get; init; }

    public int? UserNotesCount { get; init; }

    public int? Upvotes { get; init; }

    public int? Downvotes { get; init; }

    /// <summary>The short reference of the merge request within its project, such as "!7".</summary>
    public string? Reference { get; init; }

    /// <summary>The time-tracking totals. Returned on the single merge request, not on every listing.</summary>
    public GitLabTimeStats? TimeStats { get; init; }

    /// <summary>
    ///     How many files the merge request changes, as text - GitLab appends a "+" once the diff overflows
    ///     its size limit. Returned by the single-merge-request and changes endpoints.
    /// </summary>
    public string? ChangesCount { get; init; }

    /// <summary>
    ///     The per-file diffs. Populated only by <c>GET .../changes</c>; null everywhere else. The
    ///     paginated <c>GET .../diffs</c> endpoint returns the same entries on their own and is the better
    ///     choice for a large merge request.
    /// </summary>
    public IReadOnlyList<GitLabDiff>? Changes { get; init; }
}