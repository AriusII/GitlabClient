using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the PyPI package registry, sitting between the public
///     <c>IPackagesPyPiClient</c> controller and <c>IPackagesPyPiRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IPackagesPyPiService
{
    Task<GitLabFileResponse> DownloadFileForGroupAsync(GroupId groupId, string sha256, string fileIdentifier,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSimpleIndexForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSimplePackageForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    Task UploadAsync(ProjectId projectId, GitLabFileUpload content, PyPiPackageUploadRequest metadata,
        CancellationToken cancellationToken = default);

    Task AuthorizeUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadFileAsync(ProjectId projectId, string sha256, string fileIdentifier,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSimpleIndexForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSimplePackageForProjectAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);
}