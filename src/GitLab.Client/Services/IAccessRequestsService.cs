using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Access requests, sitting between the public
///     <c>IAccessRequestsClient</c> controller and <c>IAccessRequestsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IAccessRequestsService
{
    IAsyncEnumerable<GitLabAccessRequest> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabAccessRequest> RequestForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabMember> ApproveForProjectAsync(ProjectId projectId, long userId,
        ApproveAccessRequestRequest? request = null, CancellationToken cancellationToken = default);

    Task DenyForProjectAsync(ProjectId projectId, long userId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAccessRequest> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabAccessRequest> RequestForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabMember> ApproveForGroupAsync(GroupId groupId, long userId,
        ApproveAccessRequestRequest? request = null, CancellationToken cancellationToken = default);

    Task DenyForGroupAsync(GroupId groupId, long userId, CancellationToken cancellationToken = default);
}