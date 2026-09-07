using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class MergeRequestsRepository(IGitLabApiConnection connection) : IMergeRequestsRepository
{
    public Task<GitLabMergeRequest> GetAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Build(),
            GitLabJsonContext.Default.GitLabMergeRequest,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> ListAsync(ProjectId projectId, MergeRequestListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("merge_requests")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabMergeRequestArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> ListAllAsync(MergeRequestListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("merge_requests").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabMergeRequestArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> ListForGroupAsync(GroupId groupId,
        MergeRequestListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("merge_requests")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabMergeRequestArray,
            cancellationToken);
    }

    public Task<GitLabMergeRequest> CreateAsync(ProjectId projectId, CreateMergeRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("merge_requests").Build(),
            request,
            GitLabJsonContext.Default.CreateMergeRequestRequest,
            GitLabJsonContext.Default.GitLabMergeRequest,
            cancellationToken);
    }

    public Task<GitLabMergeRequest> UpdateAsync(ProjectId projectId, long mergeRequestIid,
        UpdateMergeRequestRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Build(),
            request,
            GitLabJsonContext.Default.UpdateMergeRequestRequest,
            GitLabJsonContext.Default.GitLabMergeRequest,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long mergeRequestIid, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(MergeRequestRoute(projectId, mergeRequestIid).Build(), cancellationToken);
    }

    public Task<GitLabMergeRequest> MergeAsync(ProjectId projectId, long mergeRequestIid,
        MergeMergeRequestRequest? request = null, CancellationToken cancellationToken = default)
    {
        // GitLab's merge endpoint takes every option in the body and none in the query string, so a merge
        // with no options still has to send a (empty) JSON object rather than nothing at all.
        return connection.PutAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("merge").Build(),
            request ?? new MergeMergeRequestRequest(),
            GitLabJsonContext.Default.MergeMergeRequestRequest,
            GitLabJsonContext.Default.GitLabMergeRequest,
            cancellationToken);
    }

    public Task<GitLabMergeRequestMergeRef> GetMergeRefAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("merge_ref").Build(),
            GitLabJsonContext.Default.GitLabMergeRequestMergeRef,
            cancellationToken);
    }

    public Task<GitLabMergeRequestRebaseResult> RebaseAsync(ProjectId projectId, long mergeRequestIid,
        RebaseMergeRequestRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("rebase").Build(),
            request ?? new RebaseMergeRequestRequest(),
            GitLabJsonContext.Default.RebaseMergeRequestRequest,
            GitLabJsonContext.Default.GitLabMergeRequestRebaseResult,
            cancellationToken);
    }

    public Task<GitLabMergeRequest> CancelMergeWhenPipelineSucceedsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("cancel_merge_when_pipeline_succeeds").Build(),
            GitLabJsonContext.Default.GitLabMergeRequest,
            cancellationToken);
    }

    public Task<GitLabMergeRequest> GetChangesAsync(ProjectId projectId, long mergeRequestIid, bool? unidiff = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("changes").Query("unidiff", unidiff).Build(),
            GitLabJsonContext.Default.GitLabMergeRequest,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDiff> ListDiffsAsync(ProjectId projectId, long mergeRequestIid,
        MergeRequestDiffListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("diffs").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabDiffArray,
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetRawDiffsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("raw_diffs").Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabCommit> ListCommitsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("commits").Build(),
            GitLabJsonContext.Default.GitLabCommitArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabCommit> ListContextCommitsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("context_commits").Build(),
            GitLabJsonContext.Default.GitLabCommitArray,
            cancellationToken);
    }

    public async Task<IReadOnlyList<GitLabCommit>> AddContextCommitsAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestContextCommitsRequest request, CancellationToken cancellationToken = default)
    {
        return await connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("context_commits").Build(),
            request,
            GitLabJsonContext.Default.CreateMergeRequestContextCommitsRequest,
            GitLabJsonContext.Default.GitLabCommitArray,
            cancellationToken).ConfigureAwait(false);
    }

    public Task RemoveContextCommitsAsync(ProjectId projectId, long mergeRequestIid, IReadOnlyList<string> commits,
        CancellationToken cancellationToken = default)
    {
        // The shas to drop travel in the query string, not a body: GitLab models this as a DELETE with
        // parameters.
        return connection.DeleteAsync(
            MergeRequestRoute(projectId, mergeRequestIid)
                .Literal("context_commits")
                .Query("commits", commits)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIssue> ListClosesIssuesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("closes_issues").Build(),
            GitLabJsonContext.Default.GitLabIssueArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIssue> ListRelatedIssuesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("related_issues").Build(),
            GitLabJsonContext.Default.GitLabIssueArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> ListParticipantsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("participants").Build(),
            GitLabJsonContext.Default.GitLabUserArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequestReviewer> ListReviewersAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("reviewers").Build(),
            GitLabJsonContext.Default.GitLabMergeRequestReviewerArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabPipeline> ListPipelinesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("pipelines").Build(),
            GitLabJsonContext.Default.GitLabPipelineArray,
            cancellationToken);
    }

    public Task<GitLabPipeline> CreatePipelineAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestPipelineRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("pipelines").Build(),
            request ?? new CreateMergeRequestPipelineRequest(),
            GitLabJsonContext.Default.CreateMergeRequestPipelineRequest,
            GitLabJsonContext.Default.GitLabPipeline,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequestDiffVersion> ListVersionsAsync(ProjectId projectId,
        long mergeRequestIid, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("versions").Build(),
            GitLabJsonContext.Default.GitLabMergeRequestDiffVersionArray,
            cancellationToken);
    }

    public Task<GitLabMergeRequestDiffVersion> GetVersionAsync(ProjectId projectId, long mergeRequestIid,
        long versionId, bool? unidiff = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestRoute(projectId, mergeRequestIid)
                .Literal("versions")
                .Segment(versionId)
                .Query("unidiff", unidiff)
                .Build(),
            GitLabJsonContext.Default.GitLabMergeRequestDiffVersion,
            cancellationToken);
    }

    public Task<GitLabTimeStats> SetTimeEstimateAsync(ProjectId projectId, long mergeRequestIid,
        MergeRequestTimeEstimateRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("time_estimate").Build(),
            request,
            GitLabJsonContext.Default.MergeRequestTimeEstimateRequest,
            GitLabJsonContext.Default.GitLabTimeStats,
            cancellationToken);
    }

    public Task<GitLabTimeStats> ResetTimeEstimateAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("reset_time_estimate").Build(),
            GitLabJsonContext.Default.GitLabTimeStats,
            cancellationToken);
    }

    public Task<GitLabTimeStats> AddSpentTimeAsync(ProjectId projectId, long mergeRequestIid,
        MergeRequestSpentTimeRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("add_spent_time").Build(),
            request,
            GitLabJsonContext.Default.MergeRequestSpentTimeRequest,
            GitLabJsonContext.Default.GitLabTimeStats,
            cancellationToken);
    }

    public Task<GitLabTimeStats> ResetSpentTimeAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("reset_spent_time").Build(),
            GitLabJsonContext.Default.GitLabTimeStats,
            cancellationToken);
    }

    public Task<GitLabTimeStats> GetTimeStatsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("time_stats").Build(),
            GitLabJsonContext.Default.GitLabTimeStats,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequestDependency> ListBlocksAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("blocks").Build(),
            GitLabJsonContext.Default.GitLabMergeRequestDependencyArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequestDependency> ListBlockeesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("blockees").Build(),
            GitLabJsonContext.Default.GitLabMergeRequestDependencyArray,
            cancellationToken);
    }

    public Task<GitLabMergeRequestDependency> GetBlockAsync(ProjectId projectId, long mergeRequestIid, long blockId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("blocks").Segment(blockId).Build(),
            GitLabJsonContext.Default.GitLabMergeRequestDependency,
            cancellationToken);
    }

    public Task<GitLabMergeRequestDependency> CreateBlockAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestDependencyRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("blocks").Build(),
            request,
            GitLabJsonContext.Default.CreateMergeRequestDependencyRequest,
            GitLabJsonContext.Default.GitLabMergeRequestDependency,
            cancellationToken);
    }

    public Task RemoveBlockAsync(ProjectId projectId, long mergeRequestIid, long blockId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            MergeRequestRoute(projectId, mergeRequestIid).Literal("blocks").Segment(blockId).Build(),
            cancellationToken);
    }

    /// <summary>
    ///     The <c>/projects/:id/merge_requests/:iid</c> prefix every sub-resource below hangs off. Kept as a
    ///     builder rather than a Uri so callers keep appending to it.
    /// </summary>
    private static GitLabRouteBuilder MergeRequestRoute(ProjectId projectId, long mergeRequestIid)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("merge_requests")
            .Segment(mergeRequestIid);
    }
}