using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the Terraform Module Registry, sitting between the public
///     <c>IPackagesTerraformModulesClient</c> controller and <c>IPackagesTerraformModulesRepository</c>'s
///     raw GitLab access. Mirrors the repository's method shapes 1:1 today (its implementation is
///     generated); this is the seam where request validation, caching, or cross-resource composition would
///     go once the resource needs more than pass-through.
/// </summary>
internal interface IPackagesTerraformModulesService
{
    Task<GitLabTerraformModule> GetModuleAsync(GroupId moduleNamespace, string moduleName, string moduleSystem,
        CancellationToken cancellationToken = default);

    Task<GitLabTerraformModule> GetModuleVersionAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, string moduleVersion, CancellationToken cancellationToken = default);

    Task<GitLabTerraformModuleVersionList> ListModuleVersionsAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadModuleVersionFileAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, string moduleVersion, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadModuleAsync(GroupId moduleNamespace, string moduleName, string moduleSystem,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadModuleVersionAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, string moduleVersion, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadLatestModuleAsync(ProjectId projectId, string moduleName, string moduleSystem,
        bool? terraformGet = null, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadModuleVersionAsync(ProjectId projectId, string moduleName, string moduleSystem,
        string moduleVersion, bool? terraformGet = null, CancellationToken cancellationToken = default);

    Task<GitLabTerraformModuleUploadResult> UploadModuleFileAsync(ProjectId projectId, string moduleName,
        string moduleSystem, string moduleVersion, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    Task AuthorizeModuleFileUploadAsync(ProjectId projectId, string moduleName, string moduleSystem,
        string moduleVersion, CancellationToken cancellationToken = default);
}