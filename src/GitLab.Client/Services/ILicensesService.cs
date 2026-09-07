using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Licenses, sitting between the public <c>ILicensesClient</c>
///     controller and <c>ILicensesRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ILicensesService
{
    Task<GitLabLicense> GetCurrentAsync(CancellationToken cancellationToken = default);

    Task<GitLabLicense> CreateAsync(CreateLicenseRequest request, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadUsageExportAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabLicense> ListAsync(CancellationToken cancellationToken = default);

    Task<GitLabLicense> GetAsync(long licenseId, CancellationToken cancellationToken = default);

    Task DeleteAsync(long licenseId, CancellationToken cancellationToken = default);

    Task<GitLabLicenseRefreshResult> RefreshBillableUsersAsync(long licenseId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabManagedLicense> ListManagedAsync(ProjectId projectId,
        ManagedLicenseListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabManagedLicense> GetManagedAsync(ProjectId projectId, string managedLicenseId,
        CancellationToken cancellationToken = default);

    Task<GitLabManagedLicense> CreateManagedAsync(ProjectId projectId, CreateManagedLicenseRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabManagedLicense> UpdateManagedAsync(ProjectId projectId, string managedLicenseId,
        UpdateManagedLicenseRequest request, CancellationToken cancellationToken = default);

    Task DeleteManagedAsync(ProjectId projectId, string managedLicenseId,
        CancellationToken cancellationToken = default);
}