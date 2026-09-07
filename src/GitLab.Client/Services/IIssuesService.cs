using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Issues, sitting between the public <c>IIssuesClient</c>
///     controller and <c>IIssuesRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once this resource needs more than pass-through.
/// </summary>
internal interface IIssuesService
{
    Task<GitLabIssue> GetAsync(ProjectId projectId, long issueIid, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIssue> ListAsync(ProjectId projectId, IssueListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabIssue> CreateAsync(ProjectId projectId, CreateIssueRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabIssue> UpdateAsync(ProjectId projectId, long issueIid, UpdateIssueRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabIssue> CloseAsync(ProjectId projectId, long issueIid, CancellationToken cancellationToken = default);

    Task<GitLabIssue> ReopenAsync(ProjectId projectId, long issueIid, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long issueIid, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIssue> ListForCurrentUserAsync(IssueListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabIssue> GetByIdAsync(long issueId, CancellationToken cancellationToken = default);

    Task<GitLabIssue> MoveAsync(ProjectId projectId, long issueIid, MoveIssueRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabIssue> CloneAsync(ProjectId projectId, long issueIid, CloneIssueRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabIssue> ReorderAsync(ProjectId projectId, long issueIid, ReorderIssueRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequest> ListClosedByAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequest> ListRelatedMergeRequestsAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUser> ListParticipantsAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    Task<GitLabUserAgentDetail> GetUserAgentDetailAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIssue> ListLinksAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    Task<GitLabIssueLink> CreateLinkAsync(ProjectId projectId, long issueIid, CreateIssueLinkRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabIssueLink> GetLinkAsync(ProjectId projectId, long issueIid, long issueLinkId,
        CancellationToken cancellationToken = default);

    Task DeleteLinkAsync(ProjectId projectId, long issueIid, long issueLinkId,
        CancellationToken cancellationToken = default);

    Task<GitLabTimeStats> GetTimeStatsAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    Task<GitLabTimeStats> SetTimeEstimateAsync(ProjectId projectId, long issueIid,
        SetIssueTimeEstimateRequest request, CancellationToken cancellationToken = default);

    Task<GitLabTimeStats> ResetTimeEstimateAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    Task<GitLabTimeStats> AddSpentTimeAsync(ProjectId projectId, long issueIid, AddIssueSpentTimeRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabTimeStats> ResetSpentTimeAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    Task<GitLabIssueStatistics> GetStatisticsAsync(IssueStatisticsOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabIssueStatistics> GetGroupStatisticsAsync(GroupId groupId, IssueStatisticsOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabIssueStatistics> GetProjectStatisticsAsync(ProjectId projectId, IssueStatisticsOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMetricImage> ListMetricImagesAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMetricImage> UploadMetricImageAsync(ProjectId projectId, long issueIid, GitLabFileUpload file,
        Uri? url = null, string? caption = null, CancellationToken cancellationToken = default);

    Task AuthorizeMetricImageUploadAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMetricImage> UpdateMetricImageAsync(ProjectId projectId, long issueIid, long metricImageId,
        UpdateMetricImageRequest request, CancellationToken cancellationToken = default);

    Task DeleteMetricImageAsync(ProjectId projectId, long issueIid, long metricImageId,
        CancellationToken cancellationToken = default);
}