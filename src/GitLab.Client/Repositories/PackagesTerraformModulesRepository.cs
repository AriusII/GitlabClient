using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class PackagesTerraformModulesRepository(IGitLabApiConnection connection)
    : IPackagesTerraformModulesRepository
{
    public Task<GitLabTerraformModule> GetModuleAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ModuleRoute(moduleNamespace, moduleName, moduleSystem).Build(),
            GitLabJsonContext.Default.GitLabTerraformModule,
            cancellationToken);
    }

    public Task<GitLabTerraformModule> GetModuleVersionAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, string moduleVersion, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ModuleRoute(moduleNamespace, moduleName, moduleSystem).Escaped(moduleVersion).Build(),
            GitLabJsonContext.Default.GitLabTerraformModule,
            cancellationToken);
    }

    public Task<GitLabTerraformModuleVersionList> ListModuleVersionsAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ModuleRoute(moduleNamespace, moduleName, moduleSystem).Literal("versions").Build(),
            GitLabJsonContext.Default.GitLabTerraformModuleVersionList,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadModuleVersionFileAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, string moduleVersion, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ModuleRoute(moduleNamespace, moduleName, moduleSystem).Escaped(moduleVersion).Literal("file").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadModuleAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ModuleRoute(moduleNamespace, moduleName, moduleSystem).Literal("download").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadModuleVersionAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, string moduleVersion, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ModuleRoute(moduleNamespace, moduleName, moduleSystem).Escaped(moduleVersion).Literal("download")
                .Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadLatestModuleAsync(ProjectId projectId, string moduleName,
        string moduleSystem, bool? terraformGet = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectModuleRoute(projectId, moduleName, moduleSystem).Query("terraform-get", terraformGet).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadModuleVersionAsync(ProjectId projectId, string moduleName,
        string moduleSystem, string moduleVersion, bool? terraformGet = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectModuleRoute(projectId, moduleName, moduleSystem).Escaped(moduleVersion)
                .Query("terraform-get", terraformGet).Build(),
            cancellationToken);
    }

    public Task<GitLabTerraformModuleUploadResult> UploadModuleFileAsync(ProjectId projectId, string moduleName,
        string moduleSystem, string moduleVersion, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PutFileAsync(
            ProjectModuleRoute(projectId, moduleName, moduleSystem).Escaped(moduleVersion).Literal("file").Build(),
            file,
            null,
            GitLabJsonContext.Default.GitLabTerraformModuleUploadResult,
            cancellationToken);
    }

    public Task AuthorizeModuleFileUploadAsync(ProjectId projectId, string moduleName, string moduleSystem,
        string moduleVersion, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectModuleRoute(projectId, moduleName, moduleSystem).Escaped(moduleVersion).Literal("file")
                .Literal("authorize").Build(),
            cancellationToken);
    }

    /// <summary>
    ///     <paramref name="moduleName" /> and <paramref name="moduleSystem" /> are caller-supplied free text,
    ///     so both are <c>Escaped</c>; <paramref name="moduleNamespace" /> is a group id or slug and
    ///     self-encodes via <see cref="GroupId" />.
    /// </summary>
    private static GitLabRouteBuilder ModuleRoute(GroupId moduleNamespace, string moduleName, string moduleSystem)
    {
        return GitLabRouteBuilder.Create("packages").Literal("terraform").Literal("modules").Literal("v1")
            .Segment(moduleNamespace).Escaped(moduleName).Escaped(moduleSystem);
    }

    private static GitLabRouteBuilder ProjectModuleRoute(ProjectId projectId, string moduleName, string moduleSystem)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("terraform")
            .Literal("modules").Escaped(moduleName).Escaped(moduleSystem);
    }
}