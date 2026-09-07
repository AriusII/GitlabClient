using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the RPM package registry, sitting between the public
///     <c>IPackagesRpmClient</c> controller and <c>IPackagesRpmRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IPackagesRpmService
{
    Task UploadAsync(ProjectId projectId, GitLabFileUpload file, CancellationToken cancellationToken = default);

    Task AuthorizeUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadRepositoryMetadataAsync(ProjectId projectId, string fileName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadPackageFileAsync(ProjectId projectId, long packageFileId, string fileName,
        CancellationToken cancellationToken = default);
}