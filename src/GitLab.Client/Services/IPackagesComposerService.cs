using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the Composer package registry, sitting between the public
///     <c>IPackagesComposerClient</c> controller and <c>IPackagesComposerRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this
///     is the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface IPackagesComposerService
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