using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class AnalyticsRepository(IGitLabApiConnection connection) : IAnalyticsRepository
{
    public Task<GitLabGroupIssuesCount> GetGroupActivityIssuesCountAsync(string groupPath,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("analytics").Literal("group_activity").Literal("issues_count")
                .Query("group_path", groupPath).Build(),
            GitLabJsonContext.Default.GitLabGroupIssuesCount,
            cancellationToken);
    }

    public Task<GitLabGroupMergeRequestsCount> GetGroupActivityMergeRequestsCountAsync(string groupPath,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("analytics").Literal("group_activity").Literal("merge_requests_count")
                .Query("group_path", groupPath).Build(),
            GitLabJsonContext.Default.GitLabGroupMergeRequestsCount,
            cancellationToken);
    }

    public Task<GitLabGroupNewMembersCount> GetGroupActivityNewMembersCountAsync(string groupPath,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("analytics").Literal("group_activity").Literal("new_members_count")
                .Query("group_path", groupPath).Build(),
            GitLabJsonContext.Default.GitLabGroupNewMembersCount,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDeploymentFrequency> ListDeploymentFrequencyAsync(ProjectId projectId,
        string environment, string from, DeploymentFrequencyListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("analytics")
                .Literal("deployment_frequency")
                .Query("environment", environment)
                .Query("from", from)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabDeploymentFrequencyArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabCodeReviewAnalyticsItem> ListCodeReviewAnalyticsAsync(long projectId,
        CodeReviewAnalyticsListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("analytics").Literal("code_review")
                .Query("project_id", projectId)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabCodeReviewAnalyticsItemArray,
            cancellationToken);
    }

    public Task<JsonElement> GetGroupDoraMetricsAsync(GroupId groupId, string metric,
        DoraMetricsOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("dora").Literal("metrics")
                .Query("metric", metric)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetProjectDoraMetricsAsync(ProjectId projectId, string metric,
        DoraMetricsOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("dora").Literal("metrics")
                .Query("metric", metric)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }
}