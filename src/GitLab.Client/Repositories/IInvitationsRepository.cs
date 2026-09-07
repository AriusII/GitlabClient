using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Invitations resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IInvitationsService), typeof(IInvitationsClient))]
internal interface IInvitationsRepository
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