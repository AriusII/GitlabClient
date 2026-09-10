using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class IssuesClient(IGitLabApiConnection connection) : IIssuesClient
{
    public Task<GitLabIssue> GetAsync(ProjectId projectId, long issueIid, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectIssue(projectId, issueIid).Build(),
            GitLabJsonContext.Default.GitLabIssue,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIssue> ListAsync(ProjectId projectId, IssueListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("issues")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabIssueArray,
            cancellationToken);
    }

    public Task<GitLabIssue> CreateAsync(ProjectId projectId, CreateIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("issues").Build(),
            request,
            GitLabJsonContext.Default.CreateIssueRequest,
            GitLabJsonContext.Default.GitLabIssue,
            cancellationToken);
    }

    public Task<GitLabIssue> UpdateAsync(ProjectId projectId, long issueIid, UpdateIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectIssue(projectId, issueIid).Build(),
            request,
            GitLabJsonContext.Default.UpdateIssueRequest,
            GitLabJsonContext.Default.GitLabIssue,
            cancellationToken);
    }

    public Task<GitLabIssue> CloseAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(projectId, issueIid, new UpdateIssueRequest { StateEvent = GitLabIssueStateEvent.Close },
            cancellationToken);
    }

    public Task<GitLabIssue> ReopenAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(projectId, issueIid, new UpdateIssueRequest { StateEvent = GitLabIssueStateEvent.Reopen },
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long issueIid, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(ProjectIssue(projectId, issueIid).Build(), cancellationToken);
    }

    public IAsyncEnumerable<GitLabIssue> ListForCurrentUserAsync(IssueListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("issues").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabIssueArray,
            cancellationToken);
    }

    public Task<GitLabIssue> GetByIdAsync(long issueId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("issues").Segment(issueId).Build(),
            GitLabJsonContext.Default.GitLabIssue,
            cancellationToken);
    }

    public Task<GitLabIssue> MoveAsync(ProjectId projectId, long issueIid, MoveIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectIssue(projectId, issueIid).Literal("move").Build(),
            request,
            GitLabJsonContext.Default.MoveIssueRequest,
            GitLabJsonContext.Default.GitLabIssue,
            cancellationToken);
    }

    public Task<GitLabIssue> CloneAsync(ProjectId projectId, long issueIid, CloneIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectIssue(projectId, issueIid).Literal("clone").Build(),
            request,
            GitLabJsonContext.Default.CloneIssueRequest,
            GitLabJsonContext.Default.GitLabIssue,
            cancellationToken);
    }

    public Task<GitLabIssue> ReorderAsync(ProjectId projectId, long issueIid, ReorderIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectIssue(projectId, issueIid).Literal("reorder").Build(),
            request,
            GitLabJsonContext.Default.ReorderIssueRequest,
            GitLabJsonContext.Default.GitLabIssue,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> ListClosedByAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectIssue(projectId, issueIid).Literal("closed_by").Build(),
            GitLabJsonContext.Default.GitLabMergeRequestArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> ListRelatedMergeRequestsAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectIssue(projectId, issueIid).Literal("related_merge_requests").Build(),
            GitLabJsonContext.Default.GitLabMergeRequestArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> ListParticipantsAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectIssue(projectId, issueIid).Literal("participants").Build(),
            GitLabJsonContext.Default.GitLabUserArray,
            cancellationToken);
    }

    public Task<GitLabUserAgentDetail> GetUserAgentDetailAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectIssue(projectId, issueIid).Literal("user_agent_detail").Build(),
            GitLabJsonContext.Default.GitLabUserAgentDetail,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIssue> ListLinksAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectIssue(projectId, issueIid).Literal("links").Build(),
            GitLabJsonContext.Default.GitLabIssueArray,
            cancellationToken);
    }

    public Task<GitLabIssueLink> CreateLinkAsync(ProjectId projectId, long issueIid, CreateIssueLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectIssue(projectId, issueIid).Literal("links").Build(),
            request,
            GitLabJsonContext.Default.CreateIssueLinkRequest,
            GitLabJsonContext.Default.GitLabIssueLink,
            cancellationToken);
    }

    public Task<GitLabIssueLink> GetLinkAsync(ProjectId projectId, long issueIid, long issueLinkId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectIssue(projectId, issueIid).Literal("links").Segment(issueLinkId).Build(),
            GitLabJsonContext.Default.GitLabIssueLink,
            cancellationToken);
    }

    public Task DeleteLinkAsync(ProjectId projectId, long issueIid, long issueLinkId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ProjectIssue(projectId, issueIid).Literal("links").Segment(issueLinkId).Build(),
            cancellationToken);
    }

    public Task<GitLabTimeStats> GetTimeStatsAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectIssue(projectId, issueIid).Literal("time_stats").Build(),
            GitLabJsonContext.Default.GitLabTimeStats,
            cancellationToken);
    }

    public Task<GitLabTimeStats> SetTimeEstimateAsync(ProjectId projectId, long issueIid,
        SetIssueTimeEstimateRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectIssue(projectId, issueIid).Literal("time_estimate").Build(),
            request,
            GitLabJsonContext.Default.SetIssueTimeEstimateRequest,
            GitLabJsonContext.Default.GitLabTimeStats,
            cancellationToken);
    }

    public Task<GitLabTimeStats> ResetTimeEstimateAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectIssue(projectId, issueIid).Literal("reset_time_estimate").Build(),
            GitLabJsonContext.Default.GitLabTimeStats,
            cancellationToken);
    }

    public Task<GitLabTimeStats> AddSpentTimeAsync(ProjectId projectId, long issueIid,
        AddIssueSpentTimeRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectIssue(projectId, issueIid).Literal("add_spent_time").Build(),
            request,
            GitLabJsonContext.Default.AddIssueSpentTimeRequest,
            GitLabJsonContext.Default.GitLabTimeStats,
            cancellationToken);
    }

    public Task<GitLabTimeStats> ResetSpentTimeAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectIssue(projectId, issueIid).Literal("reset_spent_time").Build(),
            GitLabJsonContext.Default.GitLabTimeStats,
            cancellationToken);
    }

    public Task<GitLabIssueStatistics> GetStatisticsAsync(IssueStatisticsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("issues_statistics").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabIssueStatistics,
            cancellationToken);
    }

    public Task<GitLabIssueStatistics> GetGroupStatisticsAsync(GroupId groupId, IssueStatisticsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("issues_statistics")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabIssueStatistics,
            cancellationToken);
    }

    public Task<GitLabIssueStatistics> GetProjectStatisticsAsync(ProjectId projectId,
        IssueStatisticsOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("issues_statistics")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabIssueStatistics,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMetricImage> ListMetricImagesAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectIssue(projectId, issueIid).Literal("metric_images").Build(),
            GitLabJsonContext.Default.GitLabMetricImageArray,
            cancellationToken);
    }

    public Task<GitLabMetricImage> UploadMetricImageAsync(ProjectId projectId, long issueIid, GitLabFileUpload file,
        Uri? url = null, string? caption = null, CancellationToken cancellationToken = default)
    {
        Dictionary<string, string>? formFields = null;

        if (url is not null || caption is not null)
        {
            formFields = new Dictionary<string, string>(StringComparer.Ordinal);

            if (url is not null)
            {
                formFields["url"] = url.ToString();
            }

            if (caption is not null)
            {
                formFields["url_text"] = caption;
            }
        }

        return connection.PostFileAsync(
            ProjectIssue(projectId, issueIid).Literal("metric_images").Build(),
            file,
            formFields,
            GitLabJsonContext.Default.GitLabMetricImage,
            cancellationToken);
    }

    public Task AuthorizeMetricImageUploadAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectIssue(projectId, issueIid).Literal("metric_images").Literal("authorize").Build(),
            cancellationToken);
    }

    public Task<GitLabMetricImage> UpdateMetricImageAsync(ProjectId projectId, long issueIid, long metricImageId,
        UpdateMetricImageRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectIssue(projectId, issueIid).Literal("metric_images").Segment(metricImageId).Build(),
            request,
            GitLabJsonContext.Default.UpdateMetricImageRequest,
            GitLabJsonContext.Default.GitLabMetricImage,
            cancellationToken);
    }

    public Task<GitLabMetricImage> DeleteMetricImageAsync(ProjectId projectId, long issueIid, long metricImageId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ProjectIssue(projectId, issueIid).Literal("metric_images").Segment(metricImageId).Build(),
            GitLabJsonContext.Default.GitLabMetricImage,
            cancellationToken);
    }

    // Every issue-scoped route in this resource starts the same way. Kept private and static so the
    // shared prefix is written once, while each caller still appends its own Literal segments.
    private static GitLabRouteBuilder ProjectIssue(ProjectId projectId, long issueIid)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("issues").Segment(issueIid);
    }
}