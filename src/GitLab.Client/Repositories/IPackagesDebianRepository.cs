using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Debian package registry (<c>/projects/:id/debian_distributions</c>,
///     <c>/groups/:id/-/debian_distributions</c>, and the APT-compatible tree under
///     <c>/projects/:id/packages/debian</c> / <c>/groups/:id/-/packages/debian</c>): builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPackagesDebianService), typeof(IPackagesDebianClient))]
internal interface IPackagesDebianRepository
{
    // ---- Distributions (project scope) ----

    IAsyncEnumerable<GitLabDebianDistribution> ListDistributionsForProjectAsync(ProjectId projectId,
        DebianDistributionListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabDebianDistribution> CreateDistributionForProjectAsync(ProjectId projectId,
        CreateDebianDistributionRequest request, CancellationToken cancellationToken = default);

    Task<GitLabDebianDistribution> GetDistributionForProjectAsync(ProjectId projectId, string codename,
        CancellationToken cancellationToken = default);

    Task<GitLabDebianDistribution> UpdateDistributionForProjectAsync(ProjectId projectId, string codename,
        UpdateDebianDistributionRequest request, CancellationToken cancellationToken = default);

    Task DeleteDistributionForProjectAsync(ProjectId projectId, string codename,
        DeleteDebianDistributionOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabDebianDistribution> GetDistributionKeyForProjectAsync(ProjectId projectId, string codename,
        CancellationToken cancellationToken = default);

    // ---- Distributions (group scope) ----

    IAsyncEnumerable<GitLabDebianDistribution> ListDistributionsForGroupAsync(GroupId groupId,
        DebianDistributionListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabDebianDistribution> CreateDistributionForGroupAsync(GroupId groupId,
        CreateDebianDistributionRequest request, CancellationToken cancellationToken = default);

    Task<GitLabDebianDistribution> GetDistributionForGroupAsync(GroupId groupId, string codename,
        CancellationToken cancellationToken = default);

    Task<GitLabDebianDistribution> UpdateDistributionForGroupAsync(GroupId groupId, string codename,
        UpdateDebianDistributionRequest request, CancellationToken cancellationToken = default);

    Task DeleteDistributionForGroupAsync(GroupId groupId, string codename,
        DeleteDebianDistributionOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabDebianDistribution> GetDistributionKeyForGroupAsync(GroupId groupId, string codename,
        CancellationToken cancellationToken = default);

    // ---- APT metadata tree (project scope) - every file here is bytes, never JSON ----

    Task<GitLabFileResponse> GetInReleaseForProjectAsync(ProjectId projectId, string distribution,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetReleaseForProjectAsync(ProjectId projectId, string distribution,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetReleaseSignatureForProjectAsync(ProjectId projectId, string distribution,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetBinaryPackagesIndexForProjectAsync(ProjectId projectId, string distribution,
        string component, string architecture, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetBinaryPackagesIndexByHashForProjectAsync(ProjectId projectId, string distribution,
        string component, string architecture, string fileSha256, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetInstallerBinaryPackagesIndexForProjectAsync(ProjectId projectId,
        string distribution, string component, string architecture, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetInstallerBinaryPackagesIndexByHashForProjectAsync(ProjectId projectId,
        string distribution, string component, string architecture, string fileSha256,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSourcePackagesIndexForProjectAsync(ProjectId projectId, string distribution,
        string component, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSourcePackagesIndexByHashForProjectAsync(ProjectId projectId, string distribution,
        string component, string fileSha256, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadPackageFileForProjectAsync(ProjectId projectId, string distribution,
        string letter, string packageName, string packageVersion, string fileName,
        CancellationToken cancellationToken = default);

    // ---- APT metadata tree (group scope) ----

    Task<GitLabFileResponse> GetInReleaseForGroupAsync(GroupId groupId, string distribution,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetReleaseForGroupAsync(GroupId groupId, string distribution,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetReleaseSignatureForGroupAsync(GroupId groupId, string distribution,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetBinaryPackagesIndexForGroupAsync(GroupId groupId, string distribution,
        string component, string architecture, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetBinaryPackagesIndexByHashForGroupAsync(GroupId groupId, string distribution,
        string component, string architecture, string fileSha256, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetInstallerBinaryPackagesIndexForGroupAsync(GroupId groupId, string distribution,
        string component, string architecture, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetInstallerBinaryPackagesIndexByHashForGroupAsync(GroupId groupId,
        string distribution, string component, string architecture, string fileSha256,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSourcePackagesIndexForGroupAsync(GroupId groupId, string distribution,
        string component, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSourcePackagesIndexByHashForGroupAsync(GroupId groupId, string distribution,
        string component, string fileSha256, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a package file from a group's Debian pool. Unlike the project-scoped download, the
    ///     group route names which project under the group actually owns the file.
    /// </summary>
    Task<GitLabFileResponse> DownloadPackageFileForGroupAsync(GroupId groupId, string distribution,
        long projectId, string letter, string packageName, string packageVersion, string fileName,
        CancellationToken cancellationToken = default);

    // ---- Package upload (project scope) ----

    /// <summary>
    ///     The pre-flight check GitLab Workhorse expects before the actual multipart upload sends the
    ///     package bytes. There is no paired upload method here - see
    ///     <see cref="IPackagesDebianClient.AuthorizePackageUploadAsync" /> for why.
    /// </summary>
    Task AuthorizePackageUploadAsync(ProjectId projectId, string fileName,
        AuthorizeDebianPackageUploadRequest request, CancellationToken cancellationToken = default);
}