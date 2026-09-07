using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;

namespace GitLab.Client.Tests.TestSupport;

/// <summary>
///     Stands in for <see cref="IMergeRequestsService" /> in the generated-layer tests. Only the three
///     members those tests drive are configurable; every other member of the (large) resource interface is
///     present purely to satisfy the compiler and throws if it is ever reached.
/// </summary>
internal sealed class FakeMergeRequestsService : IMergeRequestsService
{
    public Func<ProjectId, long, CancellationToken, Task<GitLabMergeRequest>>? OnGetAsync { get; set; }

    public Func<ProjectId, MergeRequestListOptions?, CancellationToken, IAsyncEnumerable<GitLabMergeRequest>>?
        OnListAsync
    {
        get;
        set;
    }

    public Func<ProjectId, CreateMergeRequestRequest, CancellationToken, Task<GitLabMergeRequest>>? OnCreateAsync
    {
        get;
        set;
    }

    public Task<GitLabMergeRequest> GetAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return (OnGetAsync ?? throw new InvalidOperationException($"{nameof(OnGetAsync)} was not configured."))(
            projectId, mergeRequestIid, cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> ListAsync(ProjectId projectId, MergeRequestListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return (OnListAsync ?? throw new InvalidOperationException($"{nameof(OnListAsync)} was not configured."))(
            projectId, options, cancellationToken);
    }

    public Task<GitLabMergeRequest> CreateAsync(ProjectId projectId, CreateMergeRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        return (OnCreateAsync ?? throw new InvalidOperationException($"{nameof(OnCreateAsync)} was not configured."))(
            projectId, request, cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> ListAllAsync(MergeRequestListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListAllAsync is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabMergeRequest> ListForGroupAsync(GroupId groupId,
        MergeRequestListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListForGroupAsync is not configured on this fake.");
    }

    public Task<GitLabMergeRequest> UpdateAsync(ProjectId projectId, long mergeRequestIid,
        UpdateMergeRequestRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("UpdateAsync is not configured on this fake.");
    }

    public Task DeleteAsync(ProjectId projectId, long mergeRequestIid, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("DeleteAsync is not configured on this fake.");
    }

    public Task<GitLabMergeRequest> MergeAsync(ProjectId projectId, long mergeRequestIid,
        MergeMergeRequestRequest? request = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("MergeAsync is not configured on this fake.");
    }

    public Task<GitLabMergeRequestMergeRef> GetMergeRefAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("GetMergeRefAsync is not configured on this fake.");
    }

    public Task<GitLabMergeRequestRebaseResult> RebaseAsync(ProjectId projectId, long mergeRequestIid,
        RebaseMergeRequestRequest? request = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("RebaseAsync is not configured on this fake.");
    }

    public Task<GitLabMergeRequest> CancelMergeWhenPipelineSucceedsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("CancelMergeWhenPipelineSucceedsAsync is not configured on this fake.");
    }

    public Task<GitLabMergeRequest> GetChangesAsync(ProjectId projectId, long mergeRequestIid, bool? unidiff = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("GetChangesAsync is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabDiff> ListDiffsAsync(ProjectId projectId, long mergeRequestIid,
        MergeRequestDiffListOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListDiffsAsync is not configured on this fake.");
    }

    public Task<GitLabFileResponse> GetRawDiffsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("GetRawDiffsAsync is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabCommit> ListCommitsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListCommitsAsync is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabCommit> ListContextCommitsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListContextCommitsAsync is not configured on this fake.");
    }

    public Task<IReadOnlyList<GitLabCommit>> AddContextCommitsAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestContextCommitsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("AddContextCommitsAsync is not configured on this fake.");
    }

    public Task RemoveContextCommitsAsync(ProjectId projectId, long mergeRequestIid, IReadOnlyList<string> commits,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("RemoveContextCommitsAsync is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabIssue> ListClosesIssuesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListClosesIssuesAsync is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabIssue> ListRelatedIssuesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListRelatedIssuesAsync is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabUser> ListParticipantsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListParticipantsAsync is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabMergeRequestReviewer> ListReviewersAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListReviewersAsync is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabPipeline> ListPipelinesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListPipelinesAsync is not configured on this fake.");
    }

    public Task<GitLabPipeline> CreatePipelineAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestPipelineRequest? request = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("CreatePipelineAsync is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabMergeRequestDiffVersion> ListVersionsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListVersionsAsync is not configured on this fake.");
    }

    public Task<GitLabMergeRequestDiffVersion> GetVersionAsync(ProjectId projectId, long mergeRequestIid,
        long versionId,
        bool? unidiff = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("GetVersionAsync is not configured on this fake.");
    }

    public Task<GitLabTimeStats> SetTimeEstimateAsync(ProjectId projectId, long mergeRequestIid,
        MergeRequestTimeEstimateRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("SetTimeEstimateAsync is not configured on this fake.");
    }

    public Task<GitLabTimeStats> ResetTimeEstimateAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ResetTimeEstimateAsync is not configured on this fake.");
    }

    public Task<GitLabTimeStats> AddSpentTimeAsync(ProjectId projectId, long mergeRequestIid,
        MergeRequestSpentTimeRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("AddSpentTimeAsync is not configured on this fake.");
    }

    public Task<GitLabTimeStats> ResetSpentTimeAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ResetSpentTimeAsync is not configured on this fake.");
    }

    public Task<GitLabTimeStats> GetTimeStatsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("GetTimeStatsAsync is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabMergeRequestDependency> ListBlocksAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListBlocksAsync is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabMergeRequestDependency> ListBlockeesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ListBlockeesAsync is not configured on this fake.");
    }

    public Task<GitLabMergeRequestDependency> GetBlockAsync(ProjectId projectId, long mergeRequestIid, long blockId,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("GetBlockAsync is not configured on this fake.");
    }

    public Task<GitLabMergeRequestDependency> CreateBlockAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestDependencyRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("CreateBlockAsync is not configured on this fake.");
    }

    public Task RemoveBlockAsync(ProjectId projectId, long mergeRequestIid, long blockId,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("RemoveBlockAsync is not configured on this fake.");
    }
}