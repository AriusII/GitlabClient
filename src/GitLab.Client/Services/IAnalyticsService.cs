using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Analytics, sitting between the public
///     <c>IAnalyticsClient</c> controller and <c>IAnalyticsRepository</c>'s raw GitLab access. Mirrors
///     the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IAnalyticsService
{
    Task<GitLabGroupIssuesCount> GetGroupActivityIssuesCountAsync(string groupPath,
        CancellationToken cancellationToken = default);

    Task<GitLabGroupMergeRequestsCount> GetGroupActivityMergeRequestsCountAsync(string groupPath,
        CancellationToken cancellationToken = default);

    Task<GitLabGroupNewMembersCount> GetGroupActivityNewMembersCountAsync(string groupPath,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDeploymentFrequency> ListDeploymentFrequencyAsync(ProjectId projectId,
        string environment, string from, DeploymentFrequencyListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabCodeReviewAnalyticsItem> ListCodeReviewAnalyticsAsync(long projectId,
        CodeReviewAnalyticsListOptions? options = null, CancellationToken cancellationToken = default);

    Task<JsonElement> GetGroupDoraMetricsAsync(GroupId groupId, string metric, DoraMetricsOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetProjectDoraMetricsAsync(ProjectId projectId, string metric,
        DoraMetricsOptions? options = null, CancellationToken cancellationToken = default);
}