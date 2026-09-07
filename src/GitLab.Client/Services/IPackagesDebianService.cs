using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the Debian package registry, sitting between the public
///     <c>IPackagesDebianClient</c> controller and <c>IPackagesDebianRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IPackagesDebianService
{
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

    Task<GitLabFileResponse> DownloadPackageFileForGroupAsync(GroupId groupId, string distribution, long projectId,
        string letter, string packageName, string packageVersion, string fileName,
        CancellationToken cancellationToken = default);

    Task AuthorizePackageUploadAsync(ProjectId projectId, string fileName,
        AuthorizeDebianPackageUploadRequest request, CancellationToken cancellationToken = default);
}