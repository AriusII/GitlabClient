using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class MilestonesRepository(IGitLabApiConnection connection) : IMilestonesRepository
{
    private const string MilestonesSegment = "milestones";

    public Task<GitLabMilestone> GetAsync(ProjectId projectId, long milestoneId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(MilestonesSegment).Segment(milestoneId)
                .Build(),
            GitLabJsonContext.Default.GitLabMilestone,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMilestone> ListAsync(ProjectId projectId, MilestoneListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(MilestonesSegment)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabMilestoneArray,
            cancellationToken);
    }

    public Task<GitLabMilestone> CreateAsync(ProjectId projectId, CreateMilestoneRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(MilestonesSegment).Build(),
            request,
            GitLabJsonContext.Default.CreateMilestoneRequest,
            GitLabJsonContext.Default.GitLabMilestone,
            cancellationToken);
    }

    public Task<GitLabMilestone> UpdateAsync(ProjectId projectId, long milestoneId, UpdateMilestoneRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(MilestonesSegment).Segment(milestoneId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateMilestoneRequest,
            GitLabJsonContext.Default.GitLabMilestone,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long milestoneId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(MilestonesSegment).Segment(milestoneId)
                .Build(),
            cancellationToken);
    }

    public Task PromoteAsync(ProjectId projectId, long milestoneId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(MilestonesSegment)
                .Segment(milestoneId)
                .Literal("promote")
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIssue> ListIssuesAsync(ProjectId projectId, long milestoneId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(MilestonesSegment)
                .Segment(milestoneId)
                .Literal("issues")
                .Build(),
            GitLabJsonContext.Default.GitLabIssueArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> ListMergeRequestsAsync(ProjectId projectId, long milestoneId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(MilestonesSegment)
                .Segment(milestoneId)
                .Literal("merge_requests")
                .Build(),
            GitLabJsonContext.Default.GitLabMergeRequestArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBurndownEvent> ListBurndownEventsAsync(ProjectId projectId, long milestoneId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(MilestonesSegment)
                .Segment(milestoneId)
                .Literal("burndown_events")
                .Build(),
            GitLabJsonContext.Default.GitLabBurndownEventArray,
            cancellationToken);
    }

    public Task<GitLabMilestone> GetForGroupAsync(GroupId groupId, long milestoneId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(MilestonesSegment).Segment(milestoneId)
                .Build(),
            GitLabJsonContext.Default.GitLabMilestone,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMilestone> ListForGroupAsync(GroupId groupId,
        GroupMilestoneListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(MilestonesSegment)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabMilestoneArray,
            cancellationToken);
    }

    public Task<GitLabMilestone> CreateForGroupAsync(GroupId groupId, CreateMilestoneRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(MilestonesSegment).Build(),
            request,
            GitLabJsonContext.Default.CreateMilestoneRequest,
            GitLabJsonContext.Default.GitLabMilestone,
            cancellationToken);
    }

    public Task<GitLabMilestone> UpdateForGroupAsync(GroupId groupId, long milestoneId,
        UpdateMilestoneRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(MilestonesSegment).Segment(milestoneId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateMilestoneRequest,
            GitLabJsonContext.Default.GitLabMilestone,
            cancellationToken);
    }

    public Task DeleteForGroupAsync(GroupId groupId, long milestoneId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(MilestonesSegment).Segment(milestoneId)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIssue> ListIssuesForGroupAsync(GroupId groupId, long milestoneId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(MilestonesSegment)
                .Segment(milestoneId)
                .Literal("issues")
                .Build(),
            GitLabJsonContext.Default.GitLabIssueArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> ListMergeRequestsForGroupAsync(GroupId groupId, long milestoneId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(MilestonesSegment)
                .Segment(milestoneId)
                .Literal("merge_requests")
                .Build(),
            GitLabJsonContext.Default.GitLabMergeRequestArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBurndownEvent> ListBurndownEventsForGroupAsync(GroupId groupId, long milestoneId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(MilestonesSegment)
                .Segment(milestoneId)
                .Literal("burndown_events")
                .Build(),
            GitLabJsonContext.Default.GitLabBurndownEventArray,
            cancellationToken);
    }
}