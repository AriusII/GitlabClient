using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the NuGet package registry, sitting between the public
///     <c>IPackagesNuGetClient</c> controller and <c>IPackagesNuGetRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once the resource needs
///     more than pass-through.
/// </summary>
internal interface IPackagesNuGetService
{
    Task<GitLabNugetServiceIndex> GetServiceIndexForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabNugetPackagesMetadata> GetPackageMetadataForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    Task<GitLabNugetPackageMetadata> GetPackageVersionMetadataForGroupAsync(GroupId groupId, string packageName,
        string packageVersion, CancellationToken cancellationToken = default);

    Task<GitLabNugetSearchResults> SearchForGroupAsync(GroupId groupId, NugetSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadSymbolFileForGroupAsync(GroupId groupId, string fileName, string signature,
        string sameFileName, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetV2ServiceIndexForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetV2MetadataForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task AuthorizePackageUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task UploadPackageAsync(ProjectId projectId, GitLabFileUpload package,
        CancellationToken cancellationToken = default);

    Task<GitLabNugetPackagesVersions> GetPackageVersionsAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadPackageContentAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageFilename, CancellationToken cancellationToken = default);

    Task<GitLabNugetServiceIndex> GetServiceIndexAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabNugetPackagesMetadata> GetPackageMetadataAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    Task<GitLabNugetPackageMetadata> GetPackageVersionMetadataAsync(ProjectId projectId, string packageName,
        string packageVersion, CancellationToken cancellationToken = default);

    Task<GitLabNugetSearchResults> SearchAsync(ProjectId projectId, NugetSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadSymbolFileAsync(ProjectId projectId, string fileName, string signature,
        string sameFileName, CancellationToken cancellationToken = default);

    Task AuthorizeSymbolPackageUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task UploadSymbolPackageAsync(ProjectId projectId, GitLabFileUpload symbolPackage,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetV2ServiceIndexAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetV2MetadataAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task AuthorizePackageV2UploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task UploadPackageV2Async(ProjectId projectId, GitLabFileUpload package,
        CancellationToken cancellationToken = default);

    Task DeletePackageAsync(ProjectId projectId, string packageName, string packageVersion,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> FindPackagesByIdAsync(ProjectId projectId, string id,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> EnumeratePackagesAsync(ProjectId projectId, string? filter = null,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetV2PackageMetadataAsync(ProjectId projectId, string packageName,
        string packageVersion, CancellationToken cancellationToken = default);
}