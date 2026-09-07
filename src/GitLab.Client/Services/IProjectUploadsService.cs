using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for project uploads, sitting between the public
///     <c>IProjectUploadsClient</c> controller and <c>IProjectUploadsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once the resource needs
///     more than pass-through.
/// </summary>
internal interface IProjectUploadsService
{
    IAsyncEnumerable<GitLabProjectUpload> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectUploadLink> UploadAsync(ProjectId projectId, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    Task AuthorizeUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, long uploadId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long uploadId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadBySecretAsync(ProjectId projectId, string secret, string filename,
        CancellationToken cancellationToken = default);

    Task DeleteBySecretAsync(ProjectId projectId, string secret, string filename,
        CancellationToken cancellationToken = default);
}