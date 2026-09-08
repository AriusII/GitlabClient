using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab NuGet package registry: the NuGet V3 protocol's service index, metadata and search
///     services, the legacy V2 OData feed, and the symbol server endpoint - at both project scope
///     (<c>/projects/:id/packages/nuget</c>) and group scope (<c>/groups/:id/-/packages/nuget</c>).
///     <para>
///         This is a different GitLab API from the ecosystem-agnostic "Packages" area - see
///         <see cref="IPackagesGenericClient" /> - despite both areas using the word "package"; the two
///         share no routes, DTOs or request shapes.
///     </para>
///     <para>
///         The V2 feed, <c>FindPackagesById()</c> and <c>Packages()</c> endpoints answer with an OData/XML
///         Atom feed rather than JSON, so they are exposed as raw <see cref="GitLabFileResponse" />
///         downloads like every other undocumented-schema route here.
///     </para>
/// </summary>
public interface IPackagesNuGetClient
{
    /// <summary>The NuGet V3 Feed Service Index for a group - the protocol's entry point.</summary>
    Task<GitLabNugetServiceIndex> GetServiceIndexForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>The NuGet Registration index for one package name, across every version, at group scope.</summary>
    Task<GitLabNugetPackagesMetadata> GetPackageMetadataForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>The NuGet Registration leaf for one package version, at group scope.</summary>
    Task<GitLabNugetPackageMetadata> GetPackageVersionMetadataForGroupAsync(GroupId groupId, string packageName,
        string packageVersion, CancellationToken cancellationToken = default);

    /// <summary>Searches every NuGet package published to a group.</summary>
    Task<GitLabNugetSearchResults> SearchForGroupAsync(GroupId groupId, NugetSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a <c>.pdb</c> symbol file for source-link debugging, at group scope.</summary>
    Task<GitLabFileResponse> DownloadSymbolFileForGroupAsync(GroupId groupId, string fileName, string signature,
        string sameFileName, CancellationToken cancellationToken = default);

    /// <summary>The NuGet V2 Feed Service Index for a group.</summary>
    Task<GitLabFileResponse> GetV2ServiceIndexForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>The NuGet V2 Feed's <c>$metadata</c> (CSDL) document for a group.</summary>
    Task<GitLabFileResponse> GetV2MetadataForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     The Workhorse authorize step GitLab requires immediately before uploading a NuGet v3 package to
    ///     <c>PUT /projects/:id/packages/nuget</c>.
    /// </summary>
    Task AuthorizePackageUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a NuGet v3 package (<c>.nupkg</c>) to <c>PUT /projects/:id/packages/nuget</c>. Call
    ///     <see cref="AuthorizePackageUploadAsync" /> first, as GitLab's Workhorse upload protocol requires.
    /// </summary>
    Task UploadPackageAsync(ProjectId projectId, GitLabFileUpload package,
        CancellationToken cancellationToken = default);

    /// <summary>The NuGet Content Service index - every published version of one package.</summary>
    Task<GitLabNugetPackagesVersions> GetPackageVersionsAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>The NuGet Content Service content request - downloads one package version's <c>.nupkg</c>.</summary>
    Task<GitLabFileResponse> DownloadPackageContentAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageFilename, CancellationToken cancellationToken = default);

    /// <summary>The NuGet V3 Feed Service Index for a project - the protocol's entry point.</summary>
    Task<GitLabNugetServiceIndex> GetServiceIndexAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>The NuGet Registration index for one package name, across every version, at project scope.</summary>
    Task<GitLabNugetPackagesMetadata> GetPackageMetadataAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>The NuGet Registration leaf for one package version, at project scope.</summary>
    Task<GitLabNugetPackageMetadata> GetPackageVersionMetadataAsync(ProjectId projectId, string packageName,
        string packageVersion, CancellationToken cancellationToken = default);

    /// <summary>Searches every NuGet package published to a project.</summary>
    Task<GitLabNugetSearchResults> SearchAsync(ProjectId projectId, NugetSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a <c>.pdb</c> symbol file for source-link debugging, at project scope.</summary>
    Task<GitLabFileResponse> DownloadSymbolFileAsync(ProjectId projectId, string fileName, string signature,
        string sameFileName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     The Workhorse authorize step GitLab requires immediately before uploading a <c>.snupkg</c> symbol
    ///     package.
    /// </summary>
    Task AuthorizeSymbolPackageUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a NuGet symbol package (<c>.snupkg</c>) to <c>PUT /projects/:id/packages/nuget/symbolpackage</c>.
    ///     Call <see cref="AuthorizeSymbolPackageUploadAsync" /> first, as GitLab's Workhorse upload protocol
    ///     requires.
    /// </summary>
    Task UploadSymbolPackageAsync(ProjectId projectId, GitLabFileUpload symbolPackage,
        CancellationToken cancellationToken = default);

    /// <summary>The NuGet V2 Feed Service Index for a project.</summary>
    Task<GitLabFileResponse> GetV2ServiceIndexAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>The NuGet V2 Feed's <c>$metadata</c> (CSDL) document for a project.</summary>
    Task<GitLabFileResponse> GetV2MetadataAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     The Workhorse authorize step GitLab requires immediately before uploading a NuGet v2 package to
    ///     <c>PUT /projects/:id/packages/nuget/v2</c>.
    /// </summary>
    Task AuthorizePackageV2UploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a NuGet v2 package (<c>.nupkg</c>) to <c>PUT /projects/:id/packages/nuget/v2</c>. Call
    ///     <see cref="AuthorizePackageV2UploadAsync" /> first, as GitLab's Workhorse upload protocol requires.
    /// </summary>
    Task UploadPackageV2Async(ProjectId projectId, GitLabFileUpload package,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes every version-matching package file for a NuGet package name and version.</summary>
    Task DeletePackageAsync(ProjectId projectId, string packageName, string packageVersion,
        CancellationToken cancellationToken = default);

    /// <summary>The NuGet V2 Feed <c>FindPackagesById()</c> endpoint - every version of one package by id.</summary>
    Task<GitLabFileResponse> FindPackagesByIdAsync(ProjectId projectId, string id,
        CancellationToken cancellationToken = default);

    /// <summary>The NuGet V2 Feed <c>Packages()</c> endpoint - enumerates packages, optionally OData-filtered.</summary>
    Task<GitLabFileResponse> EnumeratePackagesAsync(ProjectId projectId, string? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     The NuGet V2 Feed Single Package Metadata endpoint -
    ///     <c>Packages(Id=':package_name',Version=':package_version')</c>, an OData key predicate packed
    ///     into one path segment rather than two chained ones.
    /// </summary>
    Task<GitLabFileResponse> GetV2PackageMetadataAsync(ProjectId projectId, string packageName,
        string packageVersion, CancellationToken cancellationToken = default);
}