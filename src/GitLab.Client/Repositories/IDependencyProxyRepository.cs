using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Dependency proxy resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IDependencyProxyService), typeof(IDependencyProxyClient))]
internal interface IDependencyProxyRepository
{
    Task PurgeCacheAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadMavenPackageFileAsync(ProjectId projectId, string path, string fileName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadNpmPackageTarballAsync(ProjectId projectId, string packageName, string fileName,
        CancellationToken cancellationToken = default);
}