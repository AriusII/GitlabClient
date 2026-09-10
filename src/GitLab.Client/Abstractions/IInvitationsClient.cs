using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Invitations" API area (<c>/projects/:id/invitations</c> and
///     <c>/groups/:id/invitations</c>) - inviting people by email address or user ID, and managing the
///     invitations that have not been redeemed yet.
/// </summary>
public interface IInvitationsClient
{
    /// <summary>
    ///     Streams the project's pending invitations. Only invitations made directly on this project are
    ///     returned - invitations inherited from an ancestor group are not, so an empty result does not
    ///     mean nobody was invited.
    /// </summary>
    IAsyncEnumerable<GitLabInvitation> ListForProjectAsync(ProjectId projectId, InvitationListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Invites one or more people to a project. GitLab answers this endpoint with a status envelope
    ///     rather than the invitation itself when several invitees are involved, which is why every member
    ///     of <see cref="GitLabInvitation" /> is optional.
    /// </summary>
    Task<GitLabInvitation> CreateForProjectAsync(ProjectId projectId, CreateInvitationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a pending project invitation, addressed by the invited email address.</summary>
    Task<GitLabInvitation> UpdateForProjectAsync(ProjectId projectId, string email, UpdateInvitationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Revokes a pending project invitation, addressed by the invited email address.</summary>
    Task DeleteForProjectAsync(ProjectId projectId, string email, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the group's pending invitations. As with the project counterpart, only direct
    ///     invitations are returned; those inherited from an ancestor group are not.
    /// </summary>
    IAsyncEnumerable<GitLabInvitation> ListForGroupAsync(GroupId groupId, InvitationListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Invites one or more people to a group.</summary>
    Task<GitLabInvitation> CreateForGroupAsync(GroupId groupId, CreateInvitationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a pending group invitation, addressed by the invited email address.</summary>
    Task<GitLabInvitation> UpdateForGroupAsync(GroupId groupId, string email, UpdateInvitationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Revokes a pending group invitation, addressed by the invited email address.</summary>
    Task DeleteForGroupAsync(GroupId groupId, string email, CancellationToken cancellationToken = default);
}