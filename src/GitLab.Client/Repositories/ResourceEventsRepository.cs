using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class ResourceEventsRepository(IGitLabApiConnection connection) : IResourceEventsRepository
{
    private const string IssuesSegment = "issues";
    private const string MergeRequestsSegment = "merge_requests";
    private const string EpicsSegment = "epics";

    private const string LabelEventsSegment = "resource_label_events";
    private const string StateEventsSegment = "resource_state_events";
    private const string MilestoneEventsSegment = "resource_milestone_events";
    private const string IterationEventsSegment = "resource_iteration_events";
    private const string WeightEventsSegment = "resource_weight_events";

    public IAsyncEnumerable<GitLabResourceLabelEvent> ListIssueLabelEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectEventsRoute(projectId, IssuesSegment, issueIid, LabelEventsSegment, options),
            GitLabJsonContext.Default.GitLabResourceLabelEventArray,
            cancellationToken);
    }

    public Task<GitLabResourceLabelEvent> GetIssueLabelEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectEventRoute(projectId, IssuesSegment, issueIid, LabelEventsSegment, eventId),
            GitLabJsonContext.Default.GitLabResourceLabelEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabResourceStateEvent> ListIssueStateEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectEventsRoute(projectId, IssuesSegment, issueIid, StateEventsSegment, options),
            GitLabJsonContext.Default.GitLabResourceStateEventArray,
            cancellationToken);
    }

    public Task<GitLabResourceStateEvent> GetIssueStateEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectEventRoute(projectId, IssuesSegment, issueIid, StateEventsSegment, eventId),
            GitLabJsonContext.Default.GitLabResourceStateEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabResourceMilestoneEvent> ListIssueMilestoneEventsAsync(ProjectId projectId,
        long issueIid, ResourceEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectEventsRoute(projectId, IssuesSegment, issueIid, MilestoneEventsSegment, options),
            GitLabJsonContext.Default.GitLabResourceMilestoneEventArray,
            cancellationToken);
    }

    public Task<GitLabResourceMilestoneEvent> GetIssueMilestoneEventAsync(ProjectId projectId, long issueIid,
        long eventId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectEventRoute(projectId, IssuesSegment, issueIid, MilestoneEventsSegment, eventId),
            GitLabJsonContext.Default.GitLabResourceMilestoneEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabResourceIterationEvent> ListIssueIterationEventsAsync(ProjectId projectId,
        long issueIid, ResourceEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectEventsRoute(projectId, IssuesSegment, issueIid, IterationEventsSegment, options),
            GitLabJsonContext.Default.GitLabResourceIterationEventArray,
            cancellationToken);
    }

    public Task<GitLabResourceIterationEvent> GetIssueIterationEventAsync(ProjectId projectId, long issueIid,
        long eventId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectEventRoute(projectId, IssuesSegment, issueIid, IterationEventsSegment, eventId),
            GitLabJsonContext.Default.GitLabResourceIterationEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabResourceWeightEvent> ListIssueWeightEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectEventsRoute(projectId, IssuesSegment, issueIid, WeightEventsSegment, options),
            GitLabJsonContext.Default.GitLabResourceWeightEventArray,
            cancellationToken);
    }

    public Task<GitLabResourceWeightEvent> GetIssueWeightEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectEventRoute(projectId, IssuesSegment, issueIid, WeightEventsSegment, eventId),
            GitLabJsonContext.Default.GitLabResourceWeightEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabResourceLabelEvent> ListMergeRequestLabelEventsAsync(ProjectId projectId,
        long mergeRequestIid, ResourceEventListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectEventsRoute(projectId, MergeRequestsSegment, mergeRequestIid, LabelEventsSegment, options),
            GitLabJsonContext.Default.GitLabResourceLabelEventArray,
            cancellationToken);
    }

    public Task<GitLabResourceLabelEvent> GetMergeRequestLabelEventAsync(ProjectId projectId, long mergeRequestIid,
        long eventId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectEventRoute(projectId, MergeRequestsSegment, mergeRequestIid, LabelEventsSegment, eventId),
            GitLabJsonContext.Default.GitLabResourceLabelEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabResourceStateEvent> ListMergeRequestStateEventsAsync(ProjectId projectId,
        long mergeRequestIid, ResourceEventListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectEventsRoute(projectId, MergeRequestsSegment, mergeRequestIid, StateEventsSegment, options),
            GitLabJsonContext.Default.GitLabResourceStateEventArray,
            cancellationToken);
    }

    public Task<GitLabResourceStateEvent> GetMergeRequestStateEventAsync(ProjectId projectId, long mergeRequestIid,
        long eventId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectEventRoute(projectId, MergeRequestsSegment, mergeRequestIid, StateEventsSegment, eventId),
            GitLabJsonContext.Default.GitLabResourceStateEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabResourceMilestoneEvent> ListMergeRequestMilestoneEventsAsync(ProjectId projectId,
        long mergeRequestIid, ResourceEventListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectEventsRoute(projectId, MergeRequestsSegment, mergeRequestIid, MilestoneEventsSegment, options),
            GitLabJsonContext.Default.GitLabResourceMilestoneEventArray,
            cancellationToken);
    }

    public Task<GitLabResourceMilestoneEvent> GetMergeRequestMilestoneEventAsync(ProjectId projectId,
        long mergeRequestIid, long eventId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectEventRoute(projectId, MergeRequestsSegment, mergeRequestIid, MilestoneEventsSegment, eventId),
            GitLabJsonContext.Default.GitLabResourceMilestoneEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabResourceLabelEvent> ListEpicLabelEventsAsync(GroupId groupId, long epicIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            EpicEventsRoute(groupId, epicIid, LabelEventsSegment, options),
            GitLabJsonContext.Default.GitLabResourceLabelEventArray,
            cancellationToken);
    }

    public Task<GitLabResourceLabelEvent> GetEpicLabelEventAsync(GroupId groupId, long epicIid, long eventId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            EpicEventRoute(groupId, epicIid, LabelEventsSegment, eventId),
            GitLabJsonContext.Default.GitLabResourceLabelEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabResourceStateEvent> ListEpicStateEventsAsync(GroupId groupId, long epicIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            EpicEventsRoute(groupId, epicIid, StateEventsSegment, options),
            GitLabJsonContext.Default.GitLabResourceStateEventArray,
            cancellationToken);
    }

    public Task<GitLabResourceStateEvent> GetEpicStateEventAsync(GroupId groupId, long epicIid, long eventId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            EpicEventRoute(groupId, epicIid, StateEventsSegment, eventId),
            GitLabJsonContext.Default.GitLabResourceStateEvent,
            cancellationToken);
    }

    /// <summary>
    ///     Every project-scoped operation is the same route shape - only the eventable collection and the
    ///     event family change - so sixteen route chains collapse into these three helpers. Both string
    ///     arguments are fixed path words from the route templates, hence <c>Literal</c>; the eventable id is
    ///     the resource's <b>iid</b>, not its internal id, despite the spec naming the path parameter
    ///     <c>eventable_id</c>.
    /// </summary>
    private static Uri ProjectEventsRoute(ProjectId projectId, string eventableSegment, long eventableIid,
        string eventsSegment, ResourceEventListOptions? options)
    {
        return ProjectEventsRouteRoot(projectId, eventableSegment, eventableIid, eventsSegment)
            .QueryFrom(options)
            .Build();
    }

    private static Uri ProjectEventRoute(ProjectId projectId, string eventableSegment, long eventableIid,
        string eventsSegment, long eventId)
    {
        return ProjectEventsRouteRoot(projectId, eventableSegment, eventableIid, eventsSegment)
            .Segment(eventId)
            .Build();
    }

    private static GitLabRouteBuilder ProjectEventsRouteRoot(ProjectId projectId, string eventableSegment,
        long eventableIid, string eventsSegment)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal(eventableSegment)
            .Segment(eventableIid)
            .Literal(eventsSegment);
    }

    /// <summary>
    ///     Epic events hang off <c>/groups</c> rather than <c>/projects</c> and take a
    ///     <see cref="GroupId" /> - the one place in this resource where the eventable is not
    ///     project-scoped.
    /// </summary>
    private static Uri EpicEventsRoute(GroupId groupId, long epicIid, string eventsSegment,
        ResourceEventListOptions? options)
    {
        return EpicEventsRouteRoot(groupId, epicIid, eventsSegment)
            .QueryFrom(options)
            .Build();
    }

    private static Uri EpicEventRoute(GroupId groupId, long epicIid, string eventsSegment, long eventId)
    {
        return EpicEventsRouteRoot(groupId, epicIid, eventsSegment)
            .Segment(eventId)
            .Build();
    }

    private static GitLabRouteBuilder EpicEventsRouteRoot(GroupId groupId, long epicIid, string eventsSegment)
    {
        return GitLabRouteBuilder.Create("groups")
            .Segment(groupId)
            .Literal(EpicsSegment)
            .Segment(epicIid)
            .Literal(eventsSegment);
    }
}