using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's ecosystem-agnostic "Packages" API area: the Maven repository proxy
///     (<c>/packages/maven</c>, at instance, group and project scope), the Go module proxy
///     (<c>/projects/:id/packages/go</c>), the generic package format
///     (<c>/projects/:id/packages/generic</c>) used by consumers with no dedicated client, and the
///     format-agnostic package/package-file/pipeline management endpoints
///     (<c>/projects/:id/packages/:package_id</c>).
///     <para>
///         This is a different GitLab API from NuGet - see <see cref="IPackagesNuGetClient" /> - despite
///         both areas using the word "package"; the two share no routes, DTOs or request shapes.
///     </para>
/// </summary>
public interface IPackagesGenericClient
{
    /// <summary>Downloads a Maven package file published at group level.</summary>
    Task<GitLabFileResponse> DownloadMavenPackageFileForGroupAsync(GroupId groupId, string path, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a Maven package file through the instance-level endpoint, used when GitLab is configured
    ///     to allow duplicate Maven package names across projects.
    /// </summary>
    Task<GitLabFileResponse> DownloadMavenPackageFileAsync(string path, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a Maven package file published at project level.</summary>
    Task<GitLabFileResponse> DownloadMavenPackageFileForProjectAsync(ProjectId projectId, string path,
        string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     The Workhorse authorize step GitLab requires immediately before uploading a Maven package file to
    ///     this same route.
    /// </summary>
    Task AuthorizeMavenPackageFileUploadAsync(ProjectId projectId, string path, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a Maven package file. GitLab's Workhorse layer answers with no body, so there is nothing to
    ///     deserialize back.
    /// </summary>
    Task UploadMavenPackageFileAsync(ProjectId projectId, string path, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a generic package file.</summary>
    Task<GitLabFileResponse> DownloadGenericPackageFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a generic package file. Always asks GitLab to return the created
    ///     <see cref="GitLabPackageFile" /> (via <c>select=package_file</c>) rather than the empty body the
    ///     endpoint otherwise sends, since that is the only way to hand the caller back a typed result.
    /// </summary>
    Task<GitLabPackageFile> UploadGenericPackageFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string fileName, GitLabFileUpload file, GitLabPackageFileStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     The Workhorse authorize step GitLab requires immediately before uploading a generic package file to
    ///     this same route.
    /// </summary>
    Task AuthorizeGenericPackageFileUploadAsync(ProjectId projectId, string packageName, string packageVersion,
        string fileName, AuthorizeGenericPackageFileUploadRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>The <c>path</c>-bearing sibling of <see cref="DownloadGenericPackageFileAsync" />.</summary>
    Task<GitLabFileResponse> DownloadGenericPackageFileByPathAsync(ProjectId projectId, string packageName,
        string packageVersion, string path, string fileName, CancellationToken cancellationToken = default);

    /// <summary>The <c>path</c>-bearing sibling of <see cref="UploadGenericPackageFileAsync" />.</summary>
    Task<GitLabPackageFile> UploadGenericPackageFileByPathAsync(ProjectId projectId, string packageName,
        string packageVersion, string path, string fileName, GitLabFileUpload file,
        GitLabPackageFileStatus? status = null, CancellationToken cancellationToken = default);

    /// <summary>The <c>path</c>-bearing sibling of <see cref="AuthorizeGenericPackageFileUploadAsync" />.</summary>
    Task AuthorizeGenericPackageFileUploadByPathAsync(ProjectId projectId, string packageName,
        string packageVersion, string path, string fileName,
        AuthorizeGenericPackageFileUploadRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists every tagged version of a Go module, one per line - the Go module proxy protocol's
    ///     <c>@v/list</c> endpoint. Plain text, not JSON, so it is streamed rather than parsed.
    /// </summary>
    Task<GitLabFileResponse> ListGoModuleVersionsAsync(ProjectId projectId, string moduleName,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a Go module version's <c>.info</c> metadata (its canonical version string and commit time).</summary>
    Task<GitLabGoModuleVersionInfo> GetGoModuleVersionInfoAsync(ProjectId projectId, string moduleName,
        string moduleVersion, CancellationToken cancellationToken = default);

    /// <summary>Downloads a Go module version's <c>go.mod</c> file.</summary>
    Task<GitLabFileResponse> DownloadGoModuleFileAsync(ProjectId projectId, string moduleName,
        string moduleVersion, CancellationToken cancellationToken = default);

    /// <summary>Downloads a Go module version's full source as a zip archive.</summary>
    Task<GitLabFileResponse> DownloadGoModuleSourceAsync(ProjectId projectId, string moduleName,
        string moduleVersion, CancellationToken cancellationToken = default);

    /// <summary>Deletes a package, along with every file it owns, regardless of format.</summary>
    Task DeletePackageAsync(ProjectId projectId, long packageId, CancellationToken cancellationToken = default);

    /// <summary>Streams every file belonging to a package.</summary>
    IAsyncEnumerable<GitLabPackageFile> ListPackageFilesAsync(ProjectId projectId, long packageId,
        PackageFileListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes one file belonging to a package, leaving the package and its other files in place.</summary>
    Task DeletePackageFileAsync(ProjectId projectId, long packageId, long packageFileId,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads one package file by its numeric file ID, regardless of package format.</summary>
    Task<GitLabFileResponse> DownloadPackageFileAsync(ProjectId projectId, long packageId, long packageFileId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every pipeline that built a package, newest first.</summary>
    IAsyncEnumerable<GitLabPipeline> ListPackagePipelinesAsync(ProjectId projectId, long packageId,
        PackagePipelineListOptions? options = null, CancellationToken cancellationToken = default);
}