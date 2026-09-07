using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the ResourceEvents resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IResourceEventsService), typeof(IResourceEventsClient))]
internal interface IResourceEventsRepository
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