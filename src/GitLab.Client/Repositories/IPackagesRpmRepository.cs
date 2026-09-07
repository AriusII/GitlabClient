using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the RPM package registry
///     (<c>/projects/:id/packages/rpm/...</c>): builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls <see cref="IGitLabApiConnection" />.
///     Knows GitLab's wire format; nothing above this layer should build a route or touch
///     <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPackagesRpmService), typeof(IPackagesRpmClient))]
internal interface IPackagesRpmRepository
{
    Task UploadAsync(ProjectId projectId, GitLabFileUpload file, CancellationToken cancellationToken = default);

    Task AuthorizeUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadRepositoryMetadataAsync(ProjectId projectId, string fileName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadPackageFileAsync(ProjectId projectId, long packageFileId, string fileName,
        CancellationToken cancellationToken = default);
}