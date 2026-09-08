using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Terraform Module Registry
///     (<c>/packages/terraform/modules/v1/...</c>, <c>/projects/:id/packages/terraform/modules/...</c>) -
///     a different GitLab API area from <see cref="ITerraformStatesRepository" />, which is the Terraform
///     remote-state backend rather than a module registry. Builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls <see cref="IGitLabApiConnection" />;
///     nothing above this layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPackagesTerraformModulesService), typeof(IPackagesTerraformModulesClient))]
internal interface IPackagesTerraformModulesRepository
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