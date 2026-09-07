using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Composer package registry
///     (<c>/group/:id/-/packages/composer/...</c>, <c>/projects/:id/packages/composer/...</c>): builds
///     routes via <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPackagesComposerService), typeof(IPackagesComposerClient))]
internal interface IPackagesComposerRepository
{
    Task<GitLabFileResponse> GetRepositoryUrlTemplatesForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> ListAllForGroupAsync(GroupId groupId, string sha,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetPackageVersionsV2ForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetPackageVersionsForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    Task CreateAsync(ProjectId projectId, ComposerPackageCreateRequest? request = null,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadArchiveAsync(ProjectId projectId, string packageName, string sha,
        CancellationToken cancellationToken = default);
}