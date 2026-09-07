using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's npm registry protocol - the routes an <c>npm install</c>, <c>npm publish</c> or
///     <c>npm dist-tag</c> pointed at a GitLab-backed registry actually calls.
///     <para>
///         GitLab exposes this at three scopes that share route shapes but not a base path: project
///         (<c>/projects/:id/packages/npm/...</c>), group (<c>/groups/:id/-/packages/npm/...</c>) and
///         instance (<c>/packages/npm/...</c>, used for packages published under a top-level group's own
///         npm scope). Only the project scope can publish a tarball or download one back - group and
///         instance scope are metadata and tag-management only, mirroring what GitLab itself exposes.
///     </para>
///     <para>
///         A scoped package name (<c>@scope/name</c>) is passed exactly as npm itself writes it; GitLab's
///         npm protocol folds the scope and name into one route segment the same way an npm client does -
///         by percent-encoding the '/' between them - which every method here does automatically.
///     </para>
/// </summary>
public interface IPackagesNpmClient
{
    // ---- Project scope ----

    /// <summary>Reads a project-scoped npm package's registry metadata document (name, versions, dist-tags).</summary>
    Task<GitLabNpmPackage> GetPackageForProjectAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Publishes a new version of an npm package to a project, or deprecates one, from the raw npm
    ///     publish payload an npm client sends (GitLab accepts it as a single multipart <c>file</c> part).
    ///     GitLab declares no response schema for this endpoint, so the answer is a raw <see cref="JsonElement" />.
    /// </summary>
    Task<JsonElement> PublishForProjectAsync(ProjectId projectId, string packageName, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads one published tarball by its file name.</summary>
    Task<GitLabFileResponse> DownloadTarballForProjectAsync(ProjectId projectId, string packageName,
        string fileName, CancellationToken cancellationToken = default);

    /// <summary>Gets every dist-tag (for example <c>latest</c>) of a project-scoped npm package.</summary>
    Task<GitLabNpmDistTags> GetDistTagsForProjectAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates a dist-tag on a project-scoped npm package to point at a version.</summary>
    Task SetDistTagForProjectAsync(ProjectId projectId, string packageName, string tag,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a dist-tag from a project-scoped npm package.</summary>
    Task DeleteDistTagForProjectAsync(ProjectId projectId, string packageName, string tag,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Triggers the npm registry's bulk security-advisory endpoint for a project. GitLab declares
    ///     neither a request nor a response schema for this proxy endpoint.
    /// </summary>
    Task BulkAdvisoriesForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Triggers the npm registry's quick-audit endpoint for a project. GitLab declares neither a
    ///     request nor a response schema for this proxy endpoint.
    /// </summary>
    Task QuickAuditForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    // ---- Group scope ----

    /// <summary>Reads a group-scoped npm package's registry metadata document (name, versions, dist-tags).</summary>
    Task<GitLabNpmPackage> GetPackageForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>Gets every dist-tag (for example <c>latest</c>) of a group-scoped npm package.</summary>
    Task<GitLabNpmDistTags> GetDistTagsForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or updates a dist-tag on a group-scoped npm package to point at a version.</summary>
    Task SetDistTagForGroupAsync(GroupId groupId, string packageName, string tag,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a dist-tag from a group-scoped npm package.</summary>
    Task DeleteDistTagForGroupAsync(GroupId groupId, string packageName, string tag,
        CancellationToken cancellationToken = default);

    /// <summary>Triggers the npm registry's bulk security-advisory endpoint for a group.</summary>
    Task BulkAdvisoriesForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>Triggers the npm registry's quick-audit endpoint for a group.</summary>
    Task QuickAuditForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    // ---- Instance scope ----

    /// <summary>
    ///     Reads an instance-scoped npm package's registry metadata document - used for packages published
    ///     under a top-level group's own npm scope, with no project or group id in the route.
    /// </summary>
    Task<GitLabNpmPackage> GetPackageAsync(string packageName, CancellationToken cancellationToken = default);

    /// <summary>Gets every dist-tag (for example <c>latest</c>) of an instance-scoped npm package.</summary>
    Task<GitLabNpmDistTags> GetDistTagsAsync(string packageName, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates a dist-tag on an instance-scoped npm package to point at a version.</summary>
    Task SetDistTagAsync(string packageName, string tag, CancellationToken cancellationToken = default);

    /// <summary>Deletes a dist-tag from an instance-scoped npm package.</summary>
    Task DeleteDistTagAsync(string packageName, string tag, CancellationToken cancellationToken = default);

    /// <summary>Triggers the npm registry's instance-wide bulk security-advisory endpoint.</summary>
    Task BulkAdvisoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>Triggers the npm registry's instance-wide quick-audit endpoint.</summary>
    Task QuickAuditAsync(CancellationToken cancellationToken = default);
}