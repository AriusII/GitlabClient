using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the npm package registry resource, sitting between the public
///     <c>IPackagesNpmClient</c> controller and <c>IPackagesNpmRepository</c>'s raw GitLab access. Mirrors
///     the repository's method shapes 1:1 today (its implementation is generated); this is the seam where
///     request validation, caching, or cross-resource composition would go once the resource needs more
///     than pass-through.
/// </summary>
internal interface IPackagesNpmService
{
    Task<GitLabNpmPackage> GetPackageForProjectAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    Task<JsonElement> PublishForProjectAsync(ProjectId projectId, string packageName, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadTarballForProjectAsync(ProjectId projectId, string packageName,
        string fileName, CancellationToken cancellationToken = default);

    Task<GitLabNpmDistTags> GetDistTagsForProjectAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    Task SetDistTagForProjectAsync(ProjectId projectId, string packageName, string tag,
        CancellationToken cancellationToken = default);

    Task DeleteDistTagForProjectAsync(ProjectId projectId, string packageName, string tag,
        CancellationToken cancellationToken = default);

    Task BulkAdvisoriesForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task QuickAuditForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabNpmPackage> GetPackageForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    Task<GitLabNpmDistTags> GetDistTagsForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    Task SetDistTagForGroupAsync(GroupId groupId, string packageName, string tag,
        CancellationToken cancellationToken = default);

    Task DeleteDistTagForGroupAsync(GroupId groupId, string packageName, string tag,
        CancellationToken cancellationToken = default);

    Task BulkAdvisoriesForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task QuickAuditForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabNpmPackage> GetPackageAsync(string packageName, CancellationToken cancellationToken = default);

    Task<GitLabNpmDistTags> GetDistTagsAsync(string packageName, CancellationToken cancellationToken = default);

    Task SetDistTagAsync(string packageName, string tag, CancellationToken cancellationToken = default);

    Task DeleteDistTagAsync(string packageName, string tag, CancellationToken cancellationToken = default);

    Task BulkAdvisoriesAsync(CancellationToken cancellationToken = default);

    Task QuickAuditAsync(CancellationToken cancellationToken = default);
}