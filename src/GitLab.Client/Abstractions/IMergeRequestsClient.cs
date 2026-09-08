using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Merge Requests" API area (<c>/projects/{id}/merge_requests</c>).</summary>
public interface IMergeRequestsClient
{
    /// <summary>
    ///     Reads one merge request by its project-scoped iid (<c>GET /projects/:id/merge_requests/:iid</c>).
    /// </summary>
    Task<GitLabMergeRequest> GetAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>Streams a project's merge requests (<c>GET /projects/:id/merge_requests</c>).</summary>
    IAsyncEnumerable<GitLabMergeRequest> ListAsync(ProjectId projectId, MergeRequestListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams merge requests across the whole instance (<c>GET /merge_requests</c>). Without a
    ///     <see cref="MergeRequestListOptions.Scope" /> GitLab returns only the authenticated user's own;
    ///     pass <see cref="MergeRequestScope.All" /> for everything they can see.
    /// </summary>
    IAsyncEnumerable<GitLabMergeRequest> ListAllAsync(MergeRequestListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the merge requests of a group and all of its subgroups
    ///     (<c>GET /groups/:id/merge_requests</c>).
    /// </summary>
    IAsyncEnumerable<GitLabMergeRequest> ListForGroupAsync(GroupId groupId, MergeRequestListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Opens a new merge request (<c>POST /projects/:id/merge_requests</c>).</summary>
    Task<GitLabMergeRequest> CreateAsync(ProjectId projectId, CreateMergeRequestRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a merge request (<c>PUT /projects/:id/merge_requests/:iid</c>). Anything left null on the
    ///     request is omitted from the payload and therefore left as it is.
    /// </summary>
    Task<GitLabMergeRequest> UpdateAsync(ProjectId projectId, long mergeRequestIid,
        UpdateMergeRequestRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a merge request outright (<c>DELETE /projects/:id/merge_requests/:iid</c>). This is not
    ///     closing it - use <see cref="UpdateAsync" /> with
    ///     <see cref="MergeRequestStateEvent.Close" /> for that.
    /// </summary>
    Task DeleteAsync(ProjectId projectId, long mergeRequestIid, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Merges a merge request (<c>PUT /projects/:id/merge_requests/:iid/merge</c>) and returns it in its
    ///     merged state.
    /// </summary>
    /// <param name="projectId">The project the merge request belongs to.</param>
    /// <param name="mergeRequestIid">The merge request's project-scoped iid.</param>
    /// <param name="request">
    ///     Commit messages, squash and source-branch options. Null merges with GitLab's defaults. Set
    ///     <see cref="MergeMergeRequestRequest.Sha" /> to have GitLab refuse the merge if the source branch
    ///     has moved since it was read.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <exception cref="Exceptions.GitLabConflictException">
    ///     The merge request cannot be merged - it has conflicts, or the supplied
    ///     <see cref="MergeMergeRequestRequest.Sha" /> is no longer the head of the source branch.
    /// </exception>
    /// <exception cref="Exceptions.GitLabValidationException">
    ///     GitLab refused the merge because a merge check has not passed.
    /// </exception>
    Task<GitLabMergeRequest> MergeAsync(ProjectId projectId, long mergeRequestIid,
        MergeMergeRequestRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Merges the merge request into its merge ref without merging the target branch
    ///     (<c>GET /projects/:id/merge_requests/:iid/merge_ref</c>), and returns the sha written to
    ///     <c>refs/merge-requests/:iid/merge</c>. Used to test the merge result before committing to it.
    /// </summary>
    Task<GitLabMergeRequestMergeRef> GetMergeRefAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Queues a rebase of the source branch onto the target
    ///     (<c>PUT /projects/:id/merge_requests/:iid/rebase</c>). GitLab answers <c>202 Accepted</c> as soon
    ///     as the job is scheduled - the returned result only says the rebase started.
    /// </summary>
    Task<GitLabMergeRequestRebaseResult> RebaseAsync(ProjectId projectId, long mergeRequestIid,
        RebaseMergeRequestRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Cancels a scheduled auto-merge
    ///     (<c>POST /projects/:id/merge_requests/:iid/cancel_merge_when_pipeline_succeeds</c>).
    /// </summary>
    Task<GitLabMergeRequest> CancelMergeWhenPipelineSucceedsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads the merge request together with its per-file diffs
    ///     (<c>GET /projects/:id/merge_requests/:iid/changes</c>), which land on
    ///     <see cref="GitLabMergeRequest.Changes" />. For a large merge request prefer
    ///     <see cref="ListDiffsAsync" />, which pages.
    /// </summary>
    /// <param name="projectId">The project the merge request belongs to.</param>
    /// <param name="mergeRequestIid">The merge request's project-scoped iid.</param>
    /// <param name="unidiff">Presents the diffs in the Git unified format rather than GitLab's own.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabMergeRequest> GetChangesAsync(ProjectId projectId, long mergeRequestIid, bool? unidiff = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the merge request's per-file diffs, a page at a time
    ///     (<c>GET /projects/:id/merge_requests/:iid/diffs</c>).
    /// </summary>
    IAsyncEnumerable<GitLabDiff> ListDiffsAsync(ProjectId projectId, long mergeRequestIid,
        MergeRequestDiffListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads the merge request's diffs as a raw patch
    ///     (<c>GET /projects/:id/merge_requests/:iid/raw_diffs</c>). The body is streamed rather than parsed.
    /// </summary>
    /// <returns>
    ///     The open patch stream. The caller owns it and must dispose it - <c>await using</c> - or the
    ///     pooled connection it is streaming over is never returned.
    /// </returns>
    Task<GitLabFileResponse> GetRawDiffsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the commits on the merge request (<c>GET /projects/:id/merge_requests/:iid/commits</c>).</summary>
    IAsyncEnumerable<GitLabCommit> ListCommitsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the context commits attached to the merge request
    ///     (<c>GET /projects/:id/merge_requests/:iid/context_commits</c>) - commits shown for context that
    ///     are not part of the change itself.
    /// </summary>
    IAsyncEnumerable<GitLabCommit> ListContextCommitsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Attaches commits to the merge request as context
    ///     (<c>POST /projects/:id/merge_requests/:iid/context_commits</c>).
    /// </summary>
    Task<IReadOnlyList<GitLabCommit>> AddContextCommitsAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestContextCommitsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Detaches context commits from the merge request
    ///     (<c>DELETE /projects/:id/merge_requests/:iid/context_commits</c>).
    /// </summary>
    /// <param name="projectId">The project the merge request belongs to.</param>
    /// <param name="mergeRequestIid">The merge request's project-scoped iid.</param>
    /// <param name="commits">The shas to detach.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task RemoveContextCommitsAsync(ProjectId projectId, long mergeRequestIid, IReadOnlyList<string> commits,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the issues the merge request closes when it merges
    ///     (<c>GET /projects/:id/merge_requests/:iid/closes_issues</c>).
    /// </summary>
    IAsyncEnumerable<GitLabIssue> ListClosesIssuesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the issues related to the merge request
    ///     (<c>GET /projects/:id/merge_requests/:iid/related_issues</c>) - both the ones it closes and the
    ///     ones it merely mentions.
    /// </summary>
    IAsyncEnumerable<GitLabIssue> ListRelatedIssuesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams everyone participating in the merge request
    ///     (<c>GET /projects/:id/merge_requests/:iid/participants</c>).
    /// </summary>
    IAsyncEnumerable<GitLabUser> ListParticipantsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the merge request's reviewers together with each one's review state
    ///     (<c>GET /projects/:id/merge_requests/:iid/reviewers</c>).
    /// </summary>
    IAsyncEnumerable<GitLabMergeRequestReviewer> ListReviewersAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the pipelines run for the merge request
    ///     (<c>GET /projects/:id/merge_requests/:iid/pipelines</c>).
    /// </summary>
    IAsyncEnumerable<GitLabPipeline> ListPipelinesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Runs a new pipeline for the merge request
    ///     (<c>POST /projects/:id/merge_requests/:iid/pipelines</c>).
    /// </summary>
    Task<GitLabPipeline> CreatePipelineAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestPipelineRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the merge request's diff versions - one per push to the source branch
    ///     (<c>GET /projects/:id/merge_requests/:iid/versions</c>).
    /// </summary>
    IAsyncEnumerable<GitLabMergeRequestDiffVersion> ListVersionsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads one diff version in full, including its commits and diffs
    ///     (<c>GET /projects/:id/merge_requests/:iid/versions/:version_id</c>).
    /// </summary>
    Task<GitLabMergeRequestDiffVersion> GetVersionAsync(ProjectId projectId, long mergeRequestIid, long versionId,
        bool? unidiff = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets the merge request's time estimate
    ///     (<c>POST /projects/:id/merge_requests/:iid/time_estimate</c>).
    /// </summary>
    Task<GitLabTimeStats> SetTimeEstimateAsync(ProjectId projectId, long mergeRequestIid,
        MergeRequestTimeEstimateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Clears the merge request's time estimate
    ///     (<c>POST /projects/:id/merge_requests/:iid/reset_time_estimate</c>).
    /// </summary>
    Task<GitLabTimeStats> ResetTimeEstimateAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Logs time against the merge request
    ///     (<c>POST /projects/:id/merge_requests/:iid/add_spent_time</c>).
    /// </summary>
    Task<GitLabTimeStats> AddSpentTimeAsync(ProjectId projectId, long mergeRequestIid,
        MergeRequestSpentTimeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Clears all time logged against the merge request
    ///     (<c>POST /projects/:id/merge_requests/:iid/reset_spent_time</c>).
    /// </summary>
    Task<GitLabTimeStats> ResetSpentTimeAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads the merge request's time-tracking totals
    ///     (<c>GET /projects/:id/merge_requests/:iid/time_stats</c>).
    /// </summary>
    Task<GitLabTimeStats> GetTimeStatsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the dependencies holding this merge request back - the merge requests that must merge
    ///     first (<c>GET /projects/:id/merge_requests/:iid/blocks</c>).
    /// </summary>
    IAsyncEnumerable<GitLabMergeRequestDependency> ListBlocksAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the dependencies pointing the other way - the merge requests this one is holding back
    ///     (<c>GET /projects/:id/merge_requests/:iid/blockees</c>).
    /// </summary>
    IAsyncEnumerable<GitLabMergeRequestDependency> ListBlockeesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads one dependency by its own id (<c>GET /projects/:id/merge_requests/:iid/blocks/:block_id</c>).
    /// </summary>
    Task<GitLabMergeRequestDependency> GetBlockAsync(ProjectId projectId, long mergeRequestIid, long blockId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Records that another merge request must merge before this one
    ///     (<c>POST /projects/:id/merge_requests/:iid/blocks</c>).
    /// </summary>
    Task<GitLabMergeRequestDependency> CreateBlockAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestDependencyRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes a dependency (<c>DELETE /projects/:id/merge_requests/:iid/blocks/:block_id</c>).
    /// </summary>
    Task RemoveBlockAsync(ProjectId projectId, long mergeRequestIid, long blockId,
        CancellationToken cancellationToken = default);
}