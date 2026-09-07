using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the npm package registry resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls <see cref="IGitLabApiConnection" />.
///     Knows GitLab's wire format; nothing above this layer should build a route or touch
///     <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         GitLab exposes the npm registry protocol at three scopes that share route shapes but not a
///         base path: project (<c>/projects/:id/packages/npm/...</c>), group
///         (<c>/groups/:id/-/packages/npm/...</c>) and instance
///         (<c>/packages/npm/...</c>, used for packages published under a top-level group's own npm
///         scope). Only the project scope can publish a tarball or download one back; group and instance
///         scope are metadata/tag-management only, mirroring what GitLab itself exposes.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IPackagesNpmService), typeof(IPackagesNpmClient))]
internal interface IPackagesNpmRepository
{
    // ---- Project scope ----

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

    // ---- Group scope ----

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

    // ---- Instance scope ----

    Task<GitLabNpmPackage> GetPackageAsync(string packageName, CancellationToken cancellationToken = default);

    Task<GitLabNpmDistTags> GetDistTagsAsync(string packageName, CancellationToken cancellationToken = default);

    Task SetDistTagAsync(string packageName, string tag, CancellationToken cancellationToken = default);

    Task DeleteDistTagAsync(string packageName, string tag, CancellationToken cancellationToken = default);

    Task BulkAdvisoriesAsync(CancellationToken cancellationToken = default);

    Task QuickAuditAsync(CancellationToken cancellationToken = default);
}