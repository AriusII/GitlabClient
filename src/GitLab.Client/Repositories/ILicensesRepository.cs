using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Licenses resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ILicensesService), typeof(ILicensesClient))]
internal interface ILicensesRepository
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