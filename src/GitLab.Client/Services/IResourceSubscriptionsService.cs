using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Resource subscriptions, sitting between the public
///     <c>IResourceSubscriptionsClient</c> controller and <c>IResourceSubscriptionsRepository</c>'s raw
///     GitLab access. Mirrors the repository's method shapes 1:1 today (its implementation is
///     generated); this is the seam where request validation, caching, or cross-resource composition
///     would go once the resource needs more than pass-through.
/// </summary>
internal interface IResourceSubscriptionsService
{
    Task<GitLabIssue> SubscribeToIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    Task<GitLabIssue> UnsubscribeFromIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequest> SubscribeToMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMergeRequest> UnsubscribeFromMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabLabel> SubscribeToProjectLabelAsync(ProjectId projectId, string labelIdOrName,
        CancellationToken cancellationToken = default);

    Task<GitLabLabel> UnsubscribeFromProjectLabelAsync(ProjectId projectId, string labelIdOrName,
        CancellationToken cancellationToken = default);

    Task<GitLabLabel> SubscribeToGroupLabelAsync(GroupId groupId, string labelIdOrName,
        CancellationToken cancellationToken = default);

    Task<GitLabLabel> UnsubscribeFromGroupLabelAsync(GroupId groupId, string labelIdOrName,
        CancellationToken cancellationToken = default);
}