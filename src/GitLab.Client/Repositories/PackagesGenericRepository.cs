using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class PackagesGenericRepository(IGitLabApiConnection connection) : IPackagesGenericRepository
{
    private const string SelectPackageFile = "package_file";

    // Maven package proxy.

    public Task<GitLabFileResponse> DownloadMavenPackageFileForGroupAsync(GroupId groupId, string path,
        string fileName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("-").Literal("packages").Literal("maven")
                .Escaped(path).Escaped(fileName).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadMavenPackageFileAsync(string path, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("packages").Literal("maven").Escaped(path).Escaped(fileName).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadMavenPackageFileForProjectAsync(ProjectId projectId, string path,
        string fileName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectMavenRoute(projectId, path, fileName).Build(),
            cancellationToken);
    }

    public Task AuthorizeMavenPackageFileUploadAsync(ProjectId projectId, string path, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectMavenRoute(projectId, path, fileName).Literal("authorize").Build(),
            cancellationToken);
    }

    // Generic package format.

    public Task<GitLabFileResponse> DownloadGenericPackageFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string fileName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GenericRoute(projectId, packageName, packageVersion).Escaped(fileName).Build(),
            cancellationToken);
    }

    public Task<GitLabPackageFile> UploadGenericPackageFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string fileName, GitLabFileUpload file, GitLabPackageFileStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        return connection.PutFileAsync(
            GenericRoute(projectId, packageName, packageVersion).Escaped(fileName).Build(),
            file,
            BuildUploadFormFields(status),
            GitLabJsonContext.Default.GitLabPackageFile,
            cancellationToken);
    }

    public Task AuthorizeGenericPackageFileUploadAsync(ProjectId projectId, string packageName,
        string packageVersion, string fileName, AuthorizeGenericPackageFileUploadRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GenericRoute(projectId, packageName, packageVersion).Escaped(fileName).Literal("authorize").Build(),
            request ?? new AuthorizeGenericPackageFileUploadRequest(),
            GitLabJsonContext.Default.AuthorizeGenericPackageFileUploadRequest,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadGenericPackageFileByPathAsync(ProjectId projectId, string packageName,
        string packageVersion, string path, string fileName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GenericRoute(projectId, packageName, packageVersion).Escaped(path).Escaped(fileName).Build(),
            cancellationToken);
    }

    public Task<GitLabPackageFile> UploadGenericPackageFileByPathAsync(ProjectId projectId, string packageName,
        string packageVersion, string path, string fileName, GitLabFileUpload file,
        GitLabPackageFileStatus? status = null, CancellationToken cancellationToken = default)
    {
        return connection.PutFileAsync(
            GenericRoute(projectId, packageName, packageVersion).Escaped(path).Escaped(fileName).Build(),
            file,
            BuildUploadFormFields(status),
            GitLabJsonContext.Default.GitLabPackageFile,
            cancellationToken);
    }

    public Task AuthorizeGenericPackageFileUploadByPathAsync(ProjectId projectId, string packageName,
        string packageVersion, string path, string fileName,
        AuthorizeGenericPackageFileUploadRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GenericRoute(projectId, packageName, packageVersion).Escaped(path).Escaped(fileName)
                .Literal("authorize").Build(),
            request ?? new AuthorizeGenericPackageFileUploadRequest(),
            GitLabJsonContext.Default.AuthorizeGenericPackageFileUploadRequest,
            cancellationToken);
    }

    // Go module proxy.

    public Task<GitLabFileResponse> ListGoModuleVersionsAsync(ProjectId projectId, string moduleName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GoRoute(projectId, moduleName).Literal("list").Build(),
            cancellationToken);
    }

    public Task<GitLabGoModuleVersionInfo> GetGoModuleVersionInfoAsync(ProjectId projectId, string moduleName,
        string moduleVersion, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GoRoute(projectId, moduleName).Escaped($"{moduleVersion}.info").Build(),
            GitLabJsonContext.Default.GitLabGoModuleVersionInfo,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadGoModuleFileAsync(ProjectId projectId, string moduleName,
        string moduleVersion, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GoRoute(projectId, moduleName).Escaped($"{moduleVersion}.mod").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadGoModuleSourceAsync(ProjectId projectId, string moduleName,
        string moduleVersion, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GoRoute(projectId, moduleName).Escaped($"{moduleVersion}.zip").Build(),
            cancellationToken);
    }

    // Format-agnostic package, package file and package pipeline management.

    public Task DeletePackageAsync(ProjectId projectId, long packageId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            PackageRoute(projectId, packageId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabPackageFile> ListPackageFilesAsync(ProjectId projectId, long packageId,
        PackageFileListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            PackageRoute(projectId, packageId).Literal("package_files").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabPackageFileArray,
            cancellationToken);
    }

    public Task DeletePackageFileAsync(ProjectId projectId, long packageId, long packageFileId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            PackageRoute(projectId, packageId).Literal("package_files").Segment(packageFileId).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadPackageFileAsync(ProjectId projectId, long packageId,
        long packageFileId, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            PackageRoute(projectId, packageId).Literal("package_files").Segment(packageFileId).Literal("download")
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabPipeline> ListPackagePipelinesAsync(ProjectId projectId, long packageId,
        PackagePipelineListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            PackageRoute(projectId, packageId).Literal("pipelines").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabPipelineArray,
            cancellationToken);
    }

    private static GitLabRouteBuilder ProjectMavenRoute(ProjectId projectId, string path, string fileName)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("maven")
            .Escaped(path).Escaped(fileName);
    }

    private static GitLabRouteBuilder GenericRoute(ProjectId projectId, string packageName, string packageVersion)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("generic")
            .Escaped(packageName).Escaped(packageVersion);
    }

    private static GitLabRouteBuilder GoRoute(ProjectId projectId, string moduleName)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("go")
            .Escaped(moduleName).Literal("@v");
    }

    private static GitLabRouteBuilder PackageRoute(ProjectId projectId, long packageId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Segment(packageId);
    }

    /// <summary>
    ///     <c>select=package_file</c> is always sent so the upload answers with the created
    ///     <see cref="GitLabPackageFile" /> rather than the empty body GitLab sends by default - the only way
    ///     to get a typed result back from this endpoint through
    ///     <see cref="IGitLabApiConnection.PutFileAsync{TResponse}" />, which requires a deserializable body.
    /// </summary>
    private static Dictionary<string, string> BuildUploadFormFields(GitLabPackageFileStatus? status)
    {
        Dictionary<string, string> formFields =
            new(StringComparer.Ordinal) { ["select"] = SelectPackageFile };

        if (status is { } notNull)
        {
            formFields["status"] = notNull switch
            {
                GitLabPackageFileStatus.Default => "default",
                GitLabPackageFileStatus.Hidden => "hidden",
                _ => throw new ArgumentOutOfRangeException(nameof(status), notNull, "Unknown package file status.")
            };
        }

        return formFields;
    }
}