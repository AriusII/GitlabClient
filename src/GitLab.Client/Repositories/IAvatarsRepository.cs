using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Avatars resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IAvatarsService), typeof(IAvatarsClient))]
internal interface IAvatarsRepository
{
    Task<GitLabAvatar> GetForEmailAsync(string email, int? size = null,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);
}