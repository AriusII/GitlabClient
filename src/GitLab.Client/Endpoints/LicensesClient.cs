using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class LicensesClient(IGitLabApiConnection connection) : ILicensesClient
{
    public Task<GitLabLicense> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("license").Build(),
            GitLabJsonContext.Default.GitLabLicense,
            cancellationToken);
    }

    public Task<GitLabLicense> CreateAsync(CreateLicenseRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("license").Build(),
            request,
            GitLabJsonContext.Default.CreateLicenseRequest,
            GitLabJsonContext.Default.GitLabLicense,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadUsageExportAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("license").Literal("usage_export").Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabLicense> ListAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("licenses").Build(),
            GitLabJsonContext.Default.GitLabLicenseArray,
            cancellationToken);
    }

    public Task<GitLabLicense> GetAsync(long licenseId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("license").Segment(licenseId).Build(),
            GitLabJsonContext.Default.GitLabLicense,
            cancellationToken);
    }

    public Task DeleteAsync(long licenseId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("license").Segment(licenseId).Build(),
            cancellationToken);
    }

    public Task<GitLabLicenseRefreshResult> RefreshBillableUsersAsync(long licenseId,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("license").Segment(licenseId).Literal("refresh_billable_users").Build(),
            GitLabJsonContext.Default.GitLabLicenseRefreshResult,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabManagedLicense> ListManagedAsync(ProjectId projectId,
        ManagedLicenseListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ManagedLicensesRoute(projectId).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabManagedLicenseArray,
            cancellationToken);
    }

    public Task<GitLabManagedLicense> GetManagedAsync(ProjectId projectId, string managedLicenseId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ManagedLicensesRoute(projectId).Escaped(managedLicenseId).Build(),
            GitLabJsonContext.Default.GitLabManagedLicense,
            cancellationToken);
    }

    public Task<GitLabManagedLicense> CreateManagedAsync(ProjectId projectId, CreateManagedLicenseRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ManagedLicensesRoute(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreateManagedLicenseRequest,
            GitLabJsonContext.Default.GitLabManagedLicense,
            cancellationToken);
    }

    public Task<GitLabManagedLicense> UpdateManagedAsync(ProjectId projectId, string managedLicenseId,
        UpdateManagedLicenseRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            ManagedLicensesRoute(projectId).Escaped(managedLicenseId).Build(),
            request,
            GitLabJsonContext.Default.UpdateManagedLicenseRequest,
            GitLabJsonContext.Default.GitLabManagedLicense,
            cancellationToken);
    }

    public Task DeleteManagedAsync(ProjectId projectId, string managedLicenseId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ManagedLicensesRoute(projectId).Escaped(managedLicenseId).Build(),
            cancellationToken);
    }

    private static GitLabRouteBuilder ManagedLicensesRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("managed_licenses");
    }
}