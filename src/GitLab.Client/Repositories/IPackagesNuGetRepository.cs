using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the NuGet package registry (the V3 protocol's service index, metadata and
///     search services, plus the legacy V2 OData feed and the symbol server endpoint), at both project and
///     group scope. Builds routes via <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />; nothing above this layer should build a route or touch
///     <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPackagesNuGetService), typeof(IPackagesNuGetClient))]
internal interface IPackagesNuGetRepository
{
    // Group scope.

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

    // Project scope.

    Task AuthorizePackageUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

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

    Task<GitLabFileResponse> GetV2ServiceIndexAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetV2MetadataAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task AuthorizePackageV2UploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task DeletePackageAsync(ProjectId projectId, string packageName, string packageVersion,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> FindPackagesByIdAsync(ProjectId projectId, string id,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> EnumeratePackagesAsync(ProjectId projectId, string? filter = null,
        CancellationToken cancellationToken = default);
}