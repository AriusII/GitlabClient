using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Merge Requests resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls <see cref="IGitLabApiConnection" />.
///     Knows GitLab's wire format; nothing above this layer should build a route or touch
///     <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IMergeRequestsService), typeof(IMergeRequestsClient))]
internal interface IMergeRequestsRepository
{
    Task<GitLabMergeRequest> GetAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequest> ListAsync(ProjectId projectId, MergeRequestListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequest> ListAllAsync(MergeRequestListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequest> ListForGroupAsync(GroupId groupId, MergeRequestListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequest> CreateAsync(ProjectId projectId, CreateMergeRequestRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequest> UpdateAsync(ProjectId projectId, long mergeRequestIid,
        UpdateMergeRequestRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long mergeRequestIid, CancellationToken cancellationToken = default);

    Task<GitLabMergeRequest> MergeAsync(ProjectId projectId, long mergeRequestIid,
        MergeMergeRequestRequest? request = null, CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestMergeRef> GetMergeRefAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestRebaseResult> RebaseAsync(ProjectId projectId, long mergeRequestIid,
        RebaseMergeRequestRequest? request = null, CancellationToken cancellationToken = default);

    Task<GitLabMergeRequest> CancelMergeWhenPipelineSucceedsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequest> GetChangesAsync(ProjectId projectId, long mergeRequestIid, bool? unidiff = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDiff> ListDiffsAsync(ProjectId projectId, long mergeRequestIid,
        MergeRequestDiffListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetRawDiffsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabCommit> ListCommitsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabCommit> ListContextCommitsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GitLabCommit>> AddContextCommitsAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestContextCommitsRequest request, CancellationToken cancellationToken = default);

    Task RemoveContextCommitsAsync(ProjectId projectId, long mergeRequestIid, IReadOnlyList<string> commits,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIssue> ListClosesIssuesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIssue> ListRelatedIssuesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUser> ListParticipantsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequestReviewer> ListReviewersAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabPipeline> ListPipelinesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabPipeline> CreatePipelineAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestPipelineRequest? request = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequestDiffVersion> ListVersionsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestDiffVersion> GetVersionAsync(ProjectId projectId, long mergeRequestIid, long versionId,
        bool? unidiff = null, CancellationToken cancellationToken = default);

    Task<GitLabTimeStats> SetTimeEstimateAsync(ProjectId projectId, long mergeRequestIid,
        MergeRequestTimeEstimateRequest request, CancellationToken cancellationToken = default);

    Task<GitLabTimeStats> ResetTimeEstimateAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabTimeStats> AddSpentTimeAsync(ProjectId projectId, long mergeRequestIid,
        MergeRequestSpentTimeRequest request, CancellationToken cancellationToken = default);

    Task<GitLabTimeStats> ResetSpentTimeAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabTimeStats> GetTimeStatsAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequestDependency> ListBlocksAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequestDependency> ListBlockeesAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestDependency> GetBlockAsync(ProjectId projectId, long mergeRequestIid, long blockId,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequestDependency> CreateBlockAsync(ProjectId projectId, long mergeRequestIid,
        CreateMergeRequestDependencyRequest request, CancellationToken cancellationToken = default);

    Task RemoveBlockAsync(ProjectId projectId, long mergeRequestIid, long blockId,
        CancellationToken cancellationToken = default);
}