using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the ecosystem-agnostic "Packages" API area, sitting between the
///     public <c>IPackagesGenericClient</c> controller and <c>IPackagesGenericRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is
///     the seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IPackagesGenericService
{
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

    Task<GitLabFileResponse> ListGoModuleVersionsAsync(ProjectId projectId, string moduleName,
        CancellationToken cancellationToken = default);

    Task<GitLabGoModuleVersionInfo> GetGoModuleVersionInfoAsync(ProjectId projectId, string moduleName,
        string moduleVersion, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadGoModuleFileAsync(ProjectId projectId, string moduleName,
        string moduleVersion, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadGoModuleSourceAsync(ProjectId projectId, string moduleName,
        string moduleVersion, CancellationToken cancellationToken = default);

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