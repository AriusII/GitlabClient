using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Responses;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Resource subscriptions" API area - subscribing the authenticated user to, and
///     unsubscribing them from, issues, merge requests, project labels and group labels
///     (<c>.../subscribe</c> and <c>.../unsubscribe</c>).
///     <para>
///         Every method here returns the updated resource, whose <c>Subscribed</c> flag reports the new
///         state. None of them is idempotent from the caller's point of view: GitLab answers
///         <c>304 Not Modified</c> when the user is already in the requested state, and since 304 is not
///         a success status that surfaces as a <c>GitLabApiException</c> carrying
///         <see cref="System.Net.HttpStatusCode.NotModified" /> rather than as a no-op.
///     </para>
/// </summary>
public interface IResourceSubscriptionsClient
{
    /// <summary>
    ///     Subscribes the authenticated user to an issue
    ///     (<c>POST /projects/:id/issues/:issue_iid/subscribe</c>). Throws with
    ///     <see cref="System.Net.HttpStatusCode.NotModified" /> if already subscribed.
    /// </summary>
    Task<GitLabIssue> SubscribeToIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Unsubscribes the authenticated user from an issue
    ///     (<c>POST /projects/:id/issues/:issue_iid/unsubscribe</c>). Throws with
    ///     <see cref="System.Net.HttpStatusCode.NotModified" /> if not subscribed.
    /// </summary>
    Task<GitLabIssue> UnsubscribeFromIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Subscribes the authenticated user to a merge request
    ///     (<c>POST /projects/:id/merge_requests/:merge_request_iid/subscribe</c>). Throws with
    ///     <see cref="System.Net.HttpStatusCode.NotModified" /> if already subscribed.
    /// </summary>
    Task<GitLabMergeRequest> SubscribeToMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Unsubscribes the authenticated user from a merge request
    ///     (<c>POST /projects/:id/merge_requests/:merge_request_iid/unsubscribe</c>). Throws with
    ///     <see cref="System.Net.HttpStatusCode.NotModified" /> if not subscribed.
    /// </summary>
    Task<GitLabMergeRequest> UnsubscribeFromMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Subscribes the authenticated user to a project label
    ///     (<c>POST /projects/:id/labels/:label_id/subscribe</c>). <paramref name="labelIdOrName" /> may be
    ///     the numeric id or the label name - names routinely contain spaces and <c>::</c> scoping, and are
    ///     percent-encoded for you.
    /// </summary>
    Task<GitLabLabel> SubscribeToProjectLabelAsync(ProjectId projectId, string labelIdOrName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Unsubscribes the authenticated user from a project label
    ///     (<c>POST /projects/:id/labels/:label_id/unsubscribe</c>).
    /// </summary>
    Task<GitLabLabel> UnsubscribeFromProjectLabelAsync(ProjectId projectId, string labelIdOrName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Subscribes the authenticated user to a group label
    ///     (<c>POST /groups/:id/labels/:label_id/subscribe</c>). The response uses the distinct group-label
    ///     contract, which deliberately has no project-only <c>Priority</c> or <c>IsProjectLabel</c> member.
    /// </summary>
    Task<GitLabGroupLabel> SubscribeToGroupLabelAsync(GroupId groupId, string labelIdOrName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Unsubscribes the authenticated user from a group label
    ///     (<c>POST /groups/:id/labels/:label_id/unsubscribe</c>).
    /// </summary>
    Task<GitLabGroupLabel> UnsubscribeFromGroupLabelAsync(GroupId groupId, string labelIdOrName,
        CancellationToken cancellationToken = default);
}