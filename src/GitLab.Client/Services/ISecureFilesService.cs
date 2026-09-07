using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for CI secure files, sitting between the public
///     <c>ISecureFilesClient</c> controller and <c>ISecureFilesRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface ISecureFilesService
{
    IAsyncEnumerable<GitLabSecureFile> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabSecureFile> CreateAsync(ProjectId projectId, string name, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    Task<GitLabSecureFile> GetAsync(ProjectId projectId, long secureFileId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, long secureFileId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long secureFileId, CancellationToken cancellationToken = default);
}