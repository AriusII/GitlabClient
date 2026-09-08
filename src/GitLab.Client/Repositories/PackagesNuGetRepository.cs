using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class PackagesNuGetRepository(IGitLabApiConnection connection) : IPackagesNuGetRepository
{
    /// <summary>
    ///     The form field GitLab's NuGet package upload endpoints (v3, v2 and symbol package) read the
    ///     uploaded file from - "file", the <see cref="GitLabFileUpload" /> default, is silently ignored here.
    /// </summary>
    private const string PackageFieldName = "package";

    // Group scope.

    public Task<GitLabNugetServiceIndex> GetServiceIndexForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupNugetRoute(groupId).Literal("index").Build(),
            GitLabJsonContext.Default.GitLabNugetServiceIndex,
            cancellationToken);
    }

    public Task<GitLabNugetPackagesMetadata> GetPackageMetadataForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupNugetRoute(groupId).Literal("metadata").Escaped(packageName).Literal("index").Build(),
            GitLabJsonContext.Default.GitLabNugetPackagesMetadata,
            cancellationToken);
    }

    public Task<GitLabNugetPackageMetadata> GetPackageVersionMetadataForGroupAsync(GroupId groupId,
        string packageName, string packageVersion, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupNugetRoute(groupId).Literal("metadata").Escaped(packageName).Escaped(packageVersion).Build(),
            GitLabJsonContext.Default.GitLabNugetPackageMetadata,
            cancellationToken);
    }

    public Task<GitLabNugetSearchResults> SearchForGroupAsync(GroupId groupId, NugetSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupNugetRoute(groupId).Literal("query").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabNugetSearchResults,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadSymbolFileForGroupAsync(GroupId groupId, string fileName,
        string signature, string sameFileName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GroupNugetRoute(groupId).Literal("symbolfiles").Escaped(fileName).Escaped(signature)
                .Escaped(sameFileName).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetV2ServiceIndexForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GroupNugetRoute(groupId).Literal("v2").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetV2MetadataForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GroupNugetRoute(groupId).Literal("v2").Literal("$metadata").Build(),
            cancellationToken);
    }

    // Project scope.

    public Task AuthorizePackageUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectNugetRoute(projectId).Literal("authorize").Build(),
            cancellationToken);
    }

    public Task UploadPackageAsync(ProjectId projectId, GitLabFileUpload package,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        return connection.PutFileAsync(
            ProjectNugetRoute(projectId).Build(),
            package with { FieldName = PackageFieldName },
            null,
            cancellationToken);
    }

    public Task<GitLabNugetPackagesVersions> GetPackageVersionsAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectNugetRoute(projectId).Literal("download").Escaped(packageName).Literal("index").Build(),
            GitLabJsonContext.Default.GitLabNugetPackagesVersions,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadPackageContentAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageFilename, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectNugetRoute(projectId).Literal("download").Escaped(packageName).Escaped(packageVersion)
                .Escaped(packageFilename).Build(),
            cancellationToken);
    }

    public Task<GitLabNugetServiceIndex> GetServiceIndexAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectNugetRoute(projectId).Literal("index").Build(),
            GitLabJsonContext.Default.GitLabNugetServiceIndex,
            cancellationToken);
    }

    public Task<GitLabNugetPackagesMetadata> GetPackageMetadataAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectNugetRoute(projectId).Literal("metadata").Escaped(packageName).Literal("index").Build(),
            GitLabJsonContext.Default.GitLabNugetPackagesMetadata,
            cancellationToken);
    }

    public Task<GitLabNugetPackageMetadata> GetPackageVersionMetadataAsync(ProjectId projectId, string packageName,
        string packageVersion, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectNugetRoute(projectId).Literal("metadata").Escaped(packageName).Escaped(packageVersion).Build(),
            GitLabJsonContext.Default.GitLabNugetPackageMetadata,
            cancellationToken);
    }

    public Task<GitLabNugetSearchResults> SearchAsync(ProjectId projectId, NugetSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectNugetRoute(projectId).Literal("query").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabNugetSearchResults,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadSymbolFileAsync(ProjectId projectId, string fileName, string signature,
        string sameFileName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectNugetRoute(projectId).Literal("symbolfiles").Escaped(fileName).Escaped(signature)
                .Escaped(sameFileName).Build(),
            cancellationToken);
    }

    public Task AuthorizeSymbolPackageUploadAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectNugetRoute(projectId).Literal("symbolpackage").Literal("authorize").Build(),
            cancellationToken);
    }

    public Task UploadSymbolPackageAsync(ProjectId projectId, GitLabFileUpload symbolPackage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(symbolPackage);

        return connection.PutFileAsync(
            ProjectNugetRoute(projectId).Literal("symbolpackage").Build(),
            symbolPackage with { FieldName = PackageFieldName },
            null,
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetV2ServiceIndexAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectNugetRoute(projectId).Literal("v2").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetV2MetadataAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectNugetRoute(projectId).Literal("v2").Literal("$metadata").Build(),
            cancellationToken);
    }

    public Task AuthorizePackageV2UploadAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectNugetRoute(projectId).Literal("v2").Literal("authorize").Build(),
            cancellationToken);
    }

    public Task UploadPackageV2Async(ProjectId projectId, GitLabFileUpload package,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        return connection.PutFileAsync(
            ProjectNugetRoute(projectId).Literal("v2").Build(),
            package with { FieldName = PackageFieldName },
            null,
            cancellationToken);
    }

    public Task DeletePackageAsync(ProjectId projectId, string packageName, string packageVersion,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ProjectNugetRoute(projectId).Escaped(packageName).Escaped(packageVersion).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> FindPackagesByIdAsync(ProjectId projectId, string id,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectNugetRoute(projectId).Literal("v2").Literal("FindPackagesById()").Query("id", id).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> EnumeratePackagesAsync(ProjectId projectId, string? filter = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectNugetRoute(projectId).Literal("v2").Literal("Packages()").Query("$filter", filter).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetV2PackageMetadataAsync(ProjectId projectId, string packageName,
        string packageVersion, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectNugetRoute(projectId).Literal("v2")
                .EscapedTemplate("Packages(Id='{0}',Version='{1}')", packageName, packageVersion)
                .Build(),
            cancellationToken);
    }

    private static GitLabRouteBuilder GroupNugetRoute(GroupId groupId)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("-").Literal("packages")
            .Literal("nuget");
    }

    private static GitLabRouteBuilder ProjectNugetRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("nuget");
    }
}