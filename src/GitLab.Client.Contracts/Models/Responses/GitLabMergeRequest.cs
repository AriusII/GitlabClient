namespace GitLab.Client.Models.Responses;

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

    /// <summary>Whether GitLab created this merge request by importing it from another forge.</summary>
    public bool? Imported { get; init; }

    /// <summary>The forge this merge request was imported from, such as <c>bitbucket</c>.</summary>
    public string? ImportedFrom { get; init; }

    /// <summary>
    ///     GitLab's legacy spelling for <see cref="Draft" />. It remains available because the 19.x
    ///     response schema still emits it.
    /// </summary>
    public bool? WorkInProgress { get; init; }

    /// <summary>The milestone currently assigned to the merge request, if any.</summary>
    public GitLabMilestone? Milestone { get; init; }

    public string? MergeStatus { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    ///     The user who performed the merge, exposed by GitLab for backwards compatibility with older
    ///     clients. Prefer <see cref="MergeUser" /> in new code.
    /// </summary>
    public GitLabUser? MergedBy { get; init; }

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

    /// <summary>The earliest instant GitLab is allowed to merge this merge request.</summary>
    public DateTimeOffset? MergeAfter { get; init; }

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

    /// <summary>
    ///     The legacy singular view of <see cref="Assignees" />. GitLab keeps it for clients that predate
    ///     multiple assignees.
    /// </summary>
    public GitLabUser? Assignee { get; init; }

    public IReadOnlyList<GitLabUser>? Reviewers { get; init; }

    /// <summary>Who merged it. GitLab's replacement for the older <c>merged_by</c>.</summary>
    public GitLabUser? MergeUser { get; init; }

    public DateTimeOffset? MergedAt { get; init; }

    public GitLabUser? ClosedBy { get; init; }

    public DateTimeOffset? ClosedAt { get; init; }

    /// <summary>GitLab-rendered title HTML, included when the endpoint receives <c>render_html=true</c>.</summary>
    public string? TitleHtml { get; init; }

    /// <summary>
    ///     GitLab-rendered description HTML, included when the endpoint receives <c>render_html=true</c>.
    ///     Treat this as server-rendered HTML rather than plain text.
    /// </summary>
    public string? DescriptionHtml { get; init; }

    /// <summary>When GitLab finished preparing the merge request after it was created.</summary>
    public DateTimeOffset? PreparedAt { get; init; }

    public bool? MergeWhenPipelineSucceeds { get; init; }

    public bool? HasConflicts { get; init; }

    public bool? BlockingDiscussionsResolved { get; init; }

    public bool? DiscussionLocked { get; init; }

    public bool? Squash { get; init; }

    /// <summary>Whether GitLab will squash commits when this merge request is merged.</summary>
    public bool? SquashOnMerge { get; init; }

    /// <summary>Progress through checkbox tasks in the description.</summary>
    public GitLabTaskCompletionStatus? TaskCompletionStatus { get; init; }

    public bool? ShouldRemoveSourceBranch { get; init; }

    public bool? ForceRemoveSourceBranch { get; init; }

    public bool? AllowCollaboration { get; init; }

    /// <summary>
    ///     The legacy spelling for <see cref="AllowCollaboration" /> that is still represented in the
    ///     GitLab 19.x response schema.
    /// </summary>
    public bool? AllowMaintainerToPush { get; init; }

    public int? UserNotesCount { get; init; }

    public int? Upvotes { get; init; }

    public int? Downvotes { get; init; }

    /// <summary>The short reference of the merge request within its project, such as "!7".</summary>
    public string? Reference { get; init; }

    /// <summary>The short, relative and fully-qualified textual references for this merge request.</summary>
    public GitLabIssuableReferences? References { get; init; }

    /// <summary>The time-tracking totals. Returned on the single merge request, not on every listing.</summary>
    public GitLabTimeStats? TimeStats { get; init; }

    /// <summary>
    ///     How many files the merge request changes, as text - GitLab appends a "+" once the diff overflows
    ///     its size limit. Returned by the single-merge-request and changes endpoints.
    /// </summary>
    public string? ChangesCount { get; init; }

    /// <summary>The number of approvals GitLab requires before merging, when the project exposes it.</summary>
    public int? ApprovalsBeforeMerge { get; init; }

    /// <summary>When the latest pipeline started, if GitLab has recorded one.</summary>
    public DateTimeOffset? LatestBuildStartedAt { get; init; }

    /// <summary>When the latest pipeline finished, if GitLab has recorded one.</summary>
    public DateTimeOffset? LatestBuildFinishedAt { get; init; }

    /// <summary>When this merge request first deployed to production.</summary>
    public DateTimeOffset? FirstDeployedToProductionAt { get; init; }

    /// <summary>The pipeline GitLab associates with this merge request.</summary>
    public GitLabMergeRequestPipeline? Pipeline { get; init; }

    /// <summary>The most recent pipeline for the source branch.</summary>
    public GitLabMergeRequestPipeline? HeadPipeline { get; init; }

    /// <summary>The commit triplet that identifies the diff GitLab calculated for this merge request.</summary>
    public GitLabMergeRequestDiffRefs? DiffRefs { get; init; }

    /// <summary>The error GitLab encountered while attempting to merge, when one occurred.</summary>
    public string? MergeError { get; init; }

    /// <summary>Whether GitLab is currently rebasing the source branch.</summary>
    public bool? RebaseInProgress { get; init; }

    /// <summary>How many commits the source branch has diverged from the target branch by.</summary>
    public int? DivergedCommitsCount { get; init; }

    /// <summary>Whether the author is making their first contribution to the target project.</summary>
    public bool? FirstContribution { get; init; }

    /// <summary>The authenticated caller's merge permission for this merge request.</summary>
    public GitLabMergeRequestUser? User { get; init; }

    /// <summary>
    ///     The per-file diffs. Populated only by <c>GET .../changes</c>; null everywhere else. The
    ///     paginated <c>GET .../diffs</c> endpoint returns the same entries on their own and is the better
    ///     choice for a large merge request.
    /// </summary>
    public IReadOnlyList<GitLabDiff>? Changes { get; init; }

    /// <summary>
    ///     Whether GitLab truncated the per-file diff collection returned by <c>GET .../changes</c> because
    ///     it exceeded the server's diff limits.
    /// </summary>
    public bool? Overflow { get; init; }
}