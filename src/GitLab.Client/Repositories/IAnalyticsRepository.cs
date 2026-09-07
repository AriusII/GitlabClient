using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Analytics resource - group activity counts, project deployment
///     frequency, code review analytics and DORA metrics. Builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IAnalyticsService), typeof(IAnalyticsClient))]
internal interface IAnalyticsRepository
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