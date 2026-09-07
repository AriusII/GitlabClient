using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Resource subscriptions resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         Deliberately not named <c>ISubscriptionsRepository</c>: GitLab has a separate, fully
///         deprecated "GitLab subscriptions" (billing) tag, and the two must not be confused.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IResourceSubscriptionsService), typeof(IResourceSubscriptionsClient))]
internal interface IResourceSubscriptionsRepository
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