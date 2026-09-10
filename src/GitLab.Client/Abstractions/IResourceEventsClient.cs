using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Resource events" API area
///     (<c>/projects/:id/issues/:iid/resource_*_events</c>,
///     <c>/projects/:id/merge_requests/:iid/resource_*_events</c>,
///     <c>/groups/:id/epics/:iid/resource_*_events</c>) - the machine-readable change history of an issue,
///     merge request or epic: who added which label, who closed and reopened it, when it moved milestone,
///     iteration or weight.
///     <para>
///         Read-only, and asymmetric by design: the matrix of five event families against three eventables
///         is sparse, and only the cells GitLab actually serves are exposed here. Issues have all five
///         families; merge requests have label, state and milestone only (there is no
///         <c>resource_iteration_events</c> or <c>resource_weight_events</c> under <c>/merge_requests</c>);
///         group epics have label and state only.
///     </para>
///     <para>
///         Each family pairs a streaming list with a single-event lookup by event id. The event id is
///         GitLab's own global id for the event row, not an iid, and it is only meaningful within its own
///         family - passing a label event id to <see cref="GetIssueStateEventAsync" /> is a 404, not a
///         cross-family lookup.
///     </para>
/// </summary>
public interface IResourceEventsClient
{
    /// <summary>Streams the label add/remove history of an issue, oldest first.</summary>
    IAsyncEnumerable<GitLabResourceLabelEvent> ListIssueLabelEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one label event of an issue
    ///     (<c>GET /projects/:id/issues/:issue_iid/resource_label_events/:event_id</c>).
    /// </summary>
    Task<GitLabResourceLabelEvent> GetIssueLabelEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the close/reopen history of an issue.</summary>
    IAsyncEnumerable<GitLabResourceStateEvent> ListIssueStateEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one state event of an issue
    ///     (<c>GET /projects/:id/issues/:issue_iid/resource_state_events/:event_id</c>).
    /// </summary>
    Task<GitLabResourceStateEvent> GetIssueStateEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the milestone add/remove history of an issue.</summary>
    IAsyncEnumerable<GitLabResourceMilestoneEvent> ListIssueMilestoneEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one milestone event of an issue
    ///     (<c>GET /projects/:id/issues/:issue_iid/resource_milestone_events/:event_id</c>).
    /// </summary>
    Task<GitLabResourceMilestoneEvent> GetIssueMilestoneEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the iteration add/remove history of an issue.</summary>
    IAsyncEnumerable<GitLabResourceIterationEvent> ListIssueIterationEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one iteration event of an issue
    ///     (<c>GET /projects/:id/issues/:issue_iid/resource_iteration_events/:event_id</c>).
    /// </summary>
    Task<GitLabResourceIterationEvent> GetIssueIterationEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the weight-change history of an issue.</summary>
    IAsyncEnumerable<GitLabResourceWeightEvent> ListIssueWeightEventsAsync(ProjectId projectId, long issueIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one weight event of an issue
    ///     (<c>GET /projects/:id/issues/:issue_iid/resource_weight_events/:event_id</c>).
    /// </summary>
    Task<GitLabResourceWeightEvent> GetIssueWeightEventAsync(ProjectId projectId, long issueIid, long eventId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the label add/remove history of a merge request.</summary>
    IAsyncEnumerable<GitLabResourceLabelEvent> ListMergeRequestLabelEventsAsync(ProjectId projectId,
        long mergeRequestIid, ResourceEventListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one label event of a merge request
    ///     (<c>GET /projects/:id/merge_requests/:merge_request_iid/resource_label_events/:event_id</c>).
    /// </summary>
    Task<GitLabResourceLabelEvent> GetMergeRequestLabelEventAsync(ProjectId projectId, long mergeRequestIid,
        long eventId, CancellationToken cancellationToken = default);

    /// <summary>Streams the close/reopen/merge history of a merge request.</summary>
    IAsyncEnumerable<GitLabResourceStateEvent> ListMergeRequestStateEventsAsync(ProjectId projectId,
        long mergeRequestIid, ResourceEventListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one state event of a merge request
    ///     (<c>GET /projects/:id/merge_requests/:merge_request_iid/resource_state_events/:event_id</c>).
    /// </summary>
    Task<GitLabResourceStateEvent> GetMergeRequestStateEventAsync(ProjectId projectId, long mergeRequestIid,
        long eventId, CancellationToken cancellationToken = default);

    /// <summary>Streams the milestone add/remove history of a merge request.</summary>
    IAsyncEnumerable<GitLabResourceMilestoneEvent> ListMergeRequestMilestoneEventsAsync(ProjectId projectId,
        long mergeRequestIid, ResourceEventListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one milestone event of a merge request
    ///     (<c>GET /projects/:id/merge_requests/:merge_request_iid/resource_milestone_events/:event_id</c>).
    /// </summary>
    Task<GitLabResourceMilestoneEvent> GetMergeRequestMilestoneEventAsync(ProjectId projectId, long mergeRequestIid,
        long eventId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the label add/remove history of a group epic
    ///     (<c>GET /groups/:id/epics/:epic_iid/resource_label_events</c>). Epics themselves are superseded by
    ///     work items, but these two event feeds are not flagged deprecated in the spec and remain the only
    ///     way to read an epic's label history over REST.
    /// </summary>
    IAsyncEnumerable<GitLabResourceLabelEvent> ListEpicLabelEventsAsync(GroupId groupId, long epicIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one label event of a group epic
    ///     (<c>GET /groups/:id/epics/:epic_iid/resource_label_events/:event_id</c>).
    /// </summary>
    Task<GitLabResourceLabelEvent> GetEpicLabelEventAsync(GroupId groupId, long epicIid, long eventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the close/reopen history of a group epic
    ///     (<c>GET /groups/:id/epics/:epic_iid/resource_state_events</c>).
    /// </summary>
    IAsyncEnumerable<GitLabResourceStateEvent> ListEpicStateEventsAsync(GroupId groupId, long epicIid,
        ResourceEventListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one state event of a group epic
    ///     (<c>GET /groups/:id/epics/:epic_iid/resource_state_events/:event_id</c>).
    /// </summary>
    Task<GitLabResourceStateEvent> GetEpicStateEventAsync(GroupId groupId, long epicIid, long eventId,
        CancellationToken cancellationToken = default);
}