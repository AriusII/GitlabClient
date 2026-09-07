using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Invitations, sitting between the public
///     <c>IInvitationsClient</c> controller and <c>IInvitationsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IInvitationsService
{
    IAsyncEnumerable<GitLabInvitation> ListForProjectAsync(ProjectId projectId, InvitationListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabInvitation> CreateForProjectAsync(ProjectId projectId, CreateInvitationRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabInvitation> UpdateForProjectAsync(ProjectId projectId, string email, UpdateInvitationRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, string email, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabInvitation> ListForGroupAsync(GroupId groupId, InvitationListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabInvitation> CreateForGroupAsync(GroupId groupId, CreateInvitationRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabInvitation> UpdateForGroupAsync(GroupId groupId, string email, UpdateInvitationRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, string email, CancellationToken cancellationToken = default);
}