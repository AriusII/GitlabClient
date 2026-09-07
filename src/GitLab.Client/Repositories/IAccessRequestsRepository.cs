using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Access requests resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IAccessRequestsService), typeof(IAccessRequestsClient))]
internal interface IAccessRequestsRepository
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