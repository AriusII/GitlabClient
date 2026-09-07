using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Issues" API area: the project issue lifecycle
///     (<c>/projects/:id/issues</c>), the instance-wide listings (<c>/issues</c>), issue links, time
///     tracking, issue statistics and incident metric images.
///     <para>
///         Notes, discussions, award emoji, resource events and subscriptions hang off an issue route too,
///         but each is its own client on <see cref="IGitLabClient" />.
///     </para>
/// </summary>
public interface IIssuesClient
{
    /// <summary>Retrieves one issue by its project-scoped IID (<c>GET /projects/:id/issues/:issue_iid</c>).</summary>
    Task<GitLabIssue> GetAsync(ProjectId projectId, long issueIid, CancellationToken cancellationToken = default);

    /// <summary>Streams a project's issues (<c>GET /projects/:id/issues</c>), following the pagination links.</summary>
    IAsyncEnumerable<GitLabIssue> ListAsync(ProjectId projectId, IssueListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates an issue (<c>POST /projects/:id/issues</c>).</summary>
    Task<GitLabIssue> CreateAsync(ProjectId projectId, CreateIssueRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Applies a partial update to an issue (<c>PUT /projects/:id/issues/:issue_iid</c>). Closing and
    ///     reopening travel in <see cref="UpdateIssueRequest.StateEvent" />; see also
    ///     <see cref="CloseAsync" /> and <see cref="ReopenAsync" />.
    /// </summary>
    Task<GitLabIssue> UpdateAsync(ProjectId projectId, long issueIid, UpdateIssueRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Closes an issue - the <see cref="UpdateAsync" /> call with <c>state_event=close</c>.</summary>
    Task<GitLabIssue> CloseAsync(ProjectId projectId, long issueIid, CancellationToken cancellationToken = default);

    /// <summary>Reopens an issue - the <see cref="UpdateAsync" /> call with <c>state_event=reopen</c>.</summary>
    Task<GitLabIssue> ReopenAsync(ProjectId projectId, long issueIid, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes an issue for good (<c>DELETE /projects/:id/issues/:issue_iid</c>). Requires project owner
    ///     or administrator rights; prefer <see cref="CloseAsync" /> for ordinary workflow.
    /// </summary>
    Task DeleteAsync(ProjectId projectId, long issueIid, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the issues visible to the authenticated user across every project (<c>GET /issues</c>).
    ///     Defaults to issues they authored; widen it with <see cref="IssueListOptions.Scope" />.
    /// </summary>
    IAsyncEnumerable<GitLabIssue> ListForCurrentUserAsync(IssueListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one issue by its instance-wide id (<c>GET /issues/:id</c>), for when only the global id
    ///     is known and the owning project is not.
    /// </summary>
    Task<GitLabIssue> GetByIdAsync(long issueId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Moves an issue to another project (<c>POST /projects/:id/issues/:issue_iid/move</c>) and answers
    ///     with the issue as it now exists in the target project.
    /// </summary>
    Task<GitLabIssue> MoveAsync(ProjectId projectId, long issueIid, MoveIssueRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Copies an issue into another project (<c>POST /projects/:id/issues/:issue_iid/clone</c>), leaving
    ///     the original in place.
    /// </summary>
    Task<GitLabIssue> CloneAsync(ProjectId projectId, long issueIid, CloneIssueRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Repositions an issue in a manually ordered list
    ///     (<c>PUT /projects/:id/issues/:issue_iid/reorder</c>).
    /// </summary>
    Task<GitLabIssue> ReorderAsync(ProjectId projectId, long issueIid, ReorderIssueRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the merge requests that will close this issue when they merge
    ///     (<c>GET /projects/:id/issues/:issue_iid/closed_by</c>).
    /// </summary>
    IAsyncEnumerable<GitLabMergeRequest> ListClosedByAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every merge request that mentions this issue
    ///     (<c>GET /projects/:id/issues/:issue_iid/related_merge_requests</c>) - a superset of
    ///     <see cref="ListClosedByAsync" />.
    /// </summary>
    IAsyncEnumerable<GitLabMergeRequest> ListRelatedMergeRequestsAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the users participating in an issue
    ///     (<c>GET /projects/:id/issues/:issue_iid/participants</c>).
    /// </summary>
    IAsyncEnumerable<GitLabUser> ListParticipantsAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the spam-check record for an issue
    ///     (<c>GET /projects/:id/issues/:issue_iid/user_agent_detail</c>). Administrators only.
    /// </summary>
    Task<GitLabUserAgentDetail> GetUserAgentDetailAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the issues linked to this one (<c>GET /projects/:id/issues/:issue_iid/links</c>). GitLab
    ///     answers with the linked issues themselves, each carrying
    ///     <see cref="GitLabIssue.IssueLinkId" /> and <see cref="GitLabIssue.LinkType" />, rather than with
    ///     <see cref="GitLabIssueLink" /> objects.
    /// </summary>
    IAsyncEnumerable<GitLabIssue> ListLinksAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>Relates this issue to another (<c>POST /projects/:id/issues/:issue_iid/links</c>).</summary>
    Task<GitLabIssueLink> CreateLinkAsync(ProjectId projectId, long issueIid, CreateIssueLinkRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one relation by its link id
    ///     (<c>GET /projects/:id/issues/:issue_iid/links/:issue_link_id</c>).
    /// </summary>
    Task<GitLabIssueLink> GetLinkAsync(ProjectId projectId, long issueIid, long issueLinkId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes a relation between two issues
    ///     (<c>DELETE /projects/:id/issues/:issue_iid/links/:issue_link_id</c>). GitLab echoes the deleted
    ///     link back, but this transport does not surface a body on <c>DELETE</c>.
    /// </summary>
    Task DeleteLinkAsync(ProjectId projectId, long issueIid, long issueLinkId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the time-tracking totals for an issue
    ///     (<c>GET /projects/:id/issues/:issue_iid/time_stats</c>).
    /// </summary>
    Task<GitLabTimeStats> GetTimeStatsAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets the estimate for an issue (<c>POST /projects/:id/issues/:issue_iid/time_estimate</c>) and
    ///     answers with the updated totals.
    /// </summary>
    Task<GitLabTimeStats> SetTimeEstimateAsync(ProjectId projectId, long issueIid,
        SetIssueTimeEstimateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Clears the estimate for an issue
    ///     (<c>POST /projects/:id/issues/:issue_iid/reset_time_estimate</c>).
    /// </summary>
    Task<GitLabTimeStats> ResetTimeEstimateAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Logs time against an issue (<c>POST /projects/:id/issues/:issue_iid/add_spent_time</c>), adding to
    ///     whatever is already recorded.
    /// </summary>
    Task<GitLabTimeStats> AddSpentTimeAsync(ProjectId projectId, long issueIid, AddIssueSpentTimeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Clears all logged time on an issue
    ///     (<c>POST /projects/:id/issues/:issue_iid/reset_spent_time</c>).
    /// </summary>
    Task<GitLabTimeStats> ResetSpentTimeAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts the issues visible to the authenticated user across the instance, by state
    ///     (<c>GET /issues_statistics</c>).
    /// </summary>
    Task<GitLabIssueStatistics> GetStatisticsAsync(IssueStatisticsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Counts a group and its descendants issues by state (<c>GET /groups/:id/issues_statistics</c>).</summary>
    Task<GitLabIssueStatistics> GetGroupStatisticsAsync(GroupId groupId, IssueStatisticsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Counts a project issues by state (<c>GET /projects/:id/issues_statistics</c>).</summary>
    Task<GitLabIssueStatistics> GetProjectStatisticsAsync(ProjectId projectId, IssueStatisticsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the metric images attached to an incident
    ///     (<c>GET /projects/:id/issues/:issue_iid/metric_images</c>).
    /// </summary>
    IAsyncEnumerable<GitLabMetricImage> ListMetricImagesAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a metric image to an incident as <c>multipart/form-data</c>
    ///     (<c>POST /projects/:id/issues/:issue_iid/metric_images</c>).
    /// </summary>
    /// <param name="projectId">The project owning the incident.</param>
    /// <param name="issueIid">The incident IID.</param>
    /// <param name="file">
    ///     The image to upload. Its stream is read but not disposed, so the caller keeps ownership.
    /// </param>
    /// <param name="url">Where the metric can be seen in full, if there is such a page.</param>
    /// <param name="caption">A caption for the image or for <paramref name="url" />.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabMetricImage> UploadMetricImageAsync(ProjectId projectId, long issueIid, GitLabFileUpload file,
        Uri? url = null, string? caption = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asks GitLab Workhorse to authorize a metric image upload
    ///     (<c>POST /projects/:id/issues/:issue_iid/metric_images/authorize</c>). Only needed when driving the
    ///     two-phase Workhorse upload directly; <see cref="UploadMetricImageAsync" /> does not require it.
    /// </summary>
    Task AuthorizeMetricImageUploadAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates the link and caption of a metric image
    ///     (<c>PUT /projects/:id/issues/:issue_iid/metric_images/:metric_image_id</c>). The image itself
    ///     cannot be replaced.
    /// </summary>
    Task<GitLabMetricImage> UpdateMetricImageAsync(ProjectId projectId, long issueIid, long metricImageId,
        UpdateMetricImageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes a metric image from an incident
    ///     (<c>DELETE /projects/:id/issues/:issue_iid/metric_images/:metric_image_id</c>). GitLab echoes the
    ///     deleted image back, but this transport does not surface a body on <c>DELETE</c>.
    /// </summary>
    Task DeleteMetricImageAsync(ProjectId projectId, long issueIid, long metricImageId,
        CancellationToken cancellationToken = default);
}