using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the ecosystem-agnostic "Packages" API area: the Maven and Go package
///     proxies, the generic package format, and the format-agnostic package/package-file/pipeline
///     management endpoints. Builds routes via <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and
///     calls <see cref="IGitLabApiConnection" />; nothing above this layer should build a route or touch
///     <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPackagesGenericService), typeof(IPackagesGenericClient))]
internal interface IPackagesGenericRepository
{
    // Maven package proxy.

    Task<GitLabFileResponse> DownloadMavenPackageFileForGroupAsync(GroupId groupId, string path, string fileName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadMavenPackageFileAsync(string path, string fileName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadMavenPackageFileForProjectAsync(ProjectId projectId, string path,
        string fileName, CancellationToken cancellationToken = default);

    Task AuthorizeMavenPackageFileUploadAsync(ProjectId projectId, string path, string fileName,
        CancellationToken cancellationToken = default);

    Task UploadMavenPackageFileAsync(ProjectId projectId, string path, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    // Generic package format.

    Task<GitLabFileResponse> DownloadGenericPackageFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string fileName, CancellationToken cancellationToken = default);

    Task<GitLabPackageFile> UploadGenericPackageFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string fileName, GitLabFileUpload file, GitLabPackageFileStatus? status = null,
        CancellationToken cancellationToken = default);

    Task AuthorizeGenericPackageFileUploadAsync(ProjectId projectId, string packageName, string packageVersion,
        string fileName, AuthorizeGenericPackageFileUploadRequest? request = null,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadGenericPackageFileByPathAsync(ProjectId projectId, string packageName,
        string packageVersion, string path, string fileName, CancellationToken cancellationToken = default);

    Task<GitLabPackageFile> UploadGenericPackageFileByPathAsync(ProjectId projectId, string packageName,
        string packageVersion, string path, string fileName, GitLabFileUpload file,
        GitLabPackageFileStatus? status = null, CancellationToken cancellationToken = default);

    Task AuthorizeGenericPackageFileUploadByPathAsync(ProjectId projectId, string packageName,
        string packageVersion, string path, string fileName,
        AuthorizeGenericPackageFileUploadRequest? request = null, CancellationToken cancellationToken = default);

    // Go module proxy.

    Task<GitLabFileResponse> ListGoModuleVersionsAsync(ProjectId projectId, string moduleName,
        CancellationToken cancellationToken = default);

    Task<GitLabGoModuleVersionInfo> GetGoModuleVersionInfoAsync(ProjectId projectId, string moduleName,
        string moduleVersion, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadGoModuleFileAsync(ProjectId projectId, string moduleName,
        string moduleVersion, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadGoModuleSourceAsync(ProjectId projectId, string moduleName,
        string moduleVersion, CancellationToken cancellationToken = default);

    // Format-agnostic package, package file and package pipeline management.

    Task DeletePackageAsync(ProjectId projectId, long packageId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabPackageFile> ListPackageFilesAsync(ProjectId projectId, long packageId,
        PackageFileListOptions? options = null, CancellationToken cancellationToken = default);

    Task DeletePackageFileAsync(ProjectId projectId, long packageId, long packageFileId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadPackageFileAsync(ProjectId projectId, long packageId, long packageFileId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabPipeline> ListPackagePipelinesAsync(ProjectId projectId, long packageId,
        PackagePipelineListOptions? options = null, CancellationToken cancellationToken = default);
}