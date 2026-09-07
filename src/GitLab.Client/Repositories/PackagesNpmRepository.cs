using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class PackagesNpmRepository(IGitLabApiConnection connection) : IPackagesNpmRepository
{
    // ---- Project scope ----

    public Task<GitLabNpmPackage> GetPackageForProjectAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectPackageRoute(projectId, packageName).Build(),
            GitLabJsonContext.Default.GitLabNpmPackage,
            cancellationToken);
    }

    public Task<JsonElement> PublishForProjectAsync(ProjectId projectId, string packageName, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        return connection.PutFileAsync(
            ProjectPackageRoute(projectId, packageName).Build(),
            file,
            null,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadTarballForProjectAsync(ProjectId projectId, string packageName,
        string fileName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectPackageRoute(projectId, packageName).Literal("-").Escaped(fileName).Build(),
            cancellationToken);
    }

    public Task<GitLabNpmDistTags> GetDistTagsForProjectAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectDistTagsRoute(projectId, packageName).Build(),
            GitLabJsonContext.Default.GitLabNpmDistTags,
            cancellationToken);
    }

    public Task SetDistTagForProjectAsync(ProjectId projectId, string packageName, string tag,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectDistTagsRoute(projectId, packageName).Escaped(tag).Build(),
            cancellationToken);
    }

    public Task DeleteDistTagForProjectAsync(ProjectId projectId, string packageName, string tag,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ProjectDistTagsRoute(projectId, packageName).Escaped(tag).Build(),
            cancellationToken);
    }

    public Task BulkAdvisoriesForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectSecurityRoute(projectId).Literal("advisories").Literal("bulk").Build(),
            cancellationToken);
    }

    public Task QuickAuditForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectSecurityRoute(projectId).Literal("audits").Literal("quick").Build(),
            cancellationToken);
    }

    // ---- Group scope ----

    public Task<GitLabNpmPackage> GetPackageForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupPackageRoute(groupId, packageName).Build(),
            GitLabJsonContext.Default.GitLabNpmPackage,
            cancellationToken);
    }

    public Task<GitLabNpmDistTags> GetDistTagsForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupDistTagsRoute(groupId, packageName).Build(),
            GitLabJsonContext.Default.GitLabNpmDistTags,
            cancellationToken);
    }

    public Task SetDistTagForGroupAsync(GroupId groupId, string packageName, string tag,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GroupDistTagsRoute(groupId, packageName).Escaped(tag).Build(),
            cancellationToken);
    }

    public Task DeleteDistTagForGroupAsync(GroupId groupId, string packageName, string tag,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GroupDistTagsRoute(groupId, packageName).Escaped(tag).Build(),
            cancellationToken);
    }

    public Task BulkAdvisoriesForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GroupSecurityRoute(groupId).Literal("advisories").Literal("bulk").Build(),
            cancellationToken);
    }

    public Task QuickAuditForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GroupSecurityRoute(groupId).Literal("audits").Literal("quick").Build(),
            cancellationToken);
    }

    // ---- Instance scope ----

    public Task<GitLabNpmPackage> GetPackageAsync(string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("packages").Literal("npm").Escaped(packageName).Build(),
            GitLabJsonContext.Default.GitLabNpmPackage,
            cancellationToken);
    }

    public Task<GitLabNpmDistTags> GetDistTagsAsync(string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            InstanceDistTagsRoute(packageName).Build(),
            GitLabJsonContext.Default.GitLabNpmDistTags,
            cancellationToken);
    }

    public Task SetDistTagAsync(string packageName, string tag, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            InstanceDistTagsRoute(packageName).Escaped(tag).Build(),
            cancellationToken);
    }

    public Task DeleteDistTagAsync(string packageName, string tag, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            InstanceDistTagsRoute(packageName).Escaped(tag).Build(),
            cancellationToken);
    }

    public Task BulkAdvisoriesAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            InstanceSecurityRoute().Literal("advisories").Literal("bulk").Build(),
            cancellationToken);
    }

    public Task QuickAuditAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            InstanceSecurityRoute().Literal("audits").Literal("quick").Build(),
            cancellationToken);
    }

    // ---- Route helpers ----

    /// <summary>
    ///     <c>/projects/:id/packages/npm/:package_name</c>. <paramref name="packageName" /> is percent-encoded
    ///     as a single segment via <see cref="GitLabRouteBuilder.Escaped" />: npm's own scoped-package
    ///     convention folds <c>@scope/name</c> into one logical package identifier by percent-encoding the '/'
    ///     it contains (the same thing an npm client itself does before sending the request), so escaping the
    ///     whole string is exactly right and needs no special-casing here.
    /// </summary>
    private static GitLabRouteBuilder ProjectPackageRoute(ProjectId projectId, string packageName)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("npm")
            .Escaped(packageName);
    }

    private static GitLabRouteBuilder ProjectDistTagsRoute(ProjectId projectId, string packageName)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("npm")
            .Literal("-").Literal("package").Escaped(packageName).Literal("dist-tags");
    }

    private static GitLabRouteBuilder ProjectSecurityRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("npm")
            .Literal("-").Literal("npm").Literal("v1").Literal("security");
    }

    private static GitLabRouteBuilder GroupPackageRoute(GroupId groupId, string packageName)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("-").Literal("packages").Literal("npm")
            .Escaped(packageName);
    }

    private static GitLabRouteBuilder GroupDistTagsRoute(GroupId groupId, string packageName)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("-").Literal("packages").Literal("npm")
            .Literal("-").Literal("package").Escaped(packageName).Literal("dist-tags");
    }

    private static GitLabRouteBuilder GroupSecurityRoute(GroupId groupId)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("-").Literal("packages").Literal("npm")
            .Literal("-").Literal("npm").Literal("v1").Literal("security");
    }

    private static GitLabRouteBuilder InstanceDistTagsRoute(string packageName)
    {
        return GitLabRouteBuilder.Create("packages").Literal("npm").Literal("-").Literal("package")
            .Escaped(packageName).Literal("dist-tags");
    }

    private static GitLabRouteBuilder InstanceSecurityRoute()
    {
        return GitLabRouteBuilder.Create("packages").Literal("npm").Literal("-").Literal("npm").Literal("v1")
            .Literal("security");
    }
}