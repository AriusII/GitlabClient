using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for ResourceEvents, sitting between the public
///     <c>IResourceEventsClient</c> controller and <c>IResourceEventsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once the resource needs
///     more than pass-through.
/// </summary>
internal interface IResourceEventsService
{
    IAsyncEnumerable<GitLabResourceLabelEvent> ListIssueLabelEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabResourceLabelEvent> GetIssueLabelEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabResourceStateEvent> ListIssueStateEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabResourceStateEvent> GetIssueStateEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabResourceMilestoneEvent> ListIssueMilestoneEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabResourceMilestoneEvent> GetIssueMilestoneEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabResourceIterationEvent> ListIssueIterationEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabResourceIterationEvent> GetIssueIterationEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabResourceWeightEvent> ListIssueWeightEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabResourceWeightEvent> GetIssueWeightEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabResourceLabelEvent> ListMergeRequestLabelEventsAsync(ProjectId projectId,
        long mergeRequestIid, ResourceEventListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabResourceLabelEvent> GetMergeRequestLabelEventAsync(ProjectId projectId, long mergeRequestIid,
        long eventId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabResourceStateEvent> ListMergeRequestStateEventsAsync(ProjectId projectId,
        long mergeRequestIid, ResourceEventListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabResourceStateEvent> GetMergeRequestStateEventAsync(ProjectId projectId, long mergeRequestIid,
        long eventId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabResourceMilestoneEvent> ListMergeRequestMilestoneEventsAsync(ProjectId projectId,
        long mergeRequestIid, ResourceEventListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabResourceMilestoneEvent> GetMergeRequestMilestoneEventAsync(ProjectId projectId, long mergeRequestIid,
        long eventId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabResourceLabelEvent> ListEpicLabelEventsAsync(GroupId groupId, long epicIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabResourceLabelEvent> GetEpicLabelEventAsync(GroupId groupId, long epicIid, long eventId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabResourceStateEvent> ListEpicStateEventsAsync(GroupId groupId, long epicIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabResourceStateEvent> GetEpicStateEventAsync(GroupId groupId, long epicIid, long eventId,
        CancellationToken cancellationToken = default);
}