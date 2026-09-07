using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Licenses" API area, which covers two unrelated resources that share a tag.
///     <para>
///         The instance endpoints (<c>/license</c>, <c>/licenses</c>) manage the key that activates
///         GitLab Enterprise Edition. They require administrator access, and they are the reason
///         <see cref="CreateAsync" /> takes a dedicated request record: the licence key is
///         credential-adjacent, so it is never logged, never echoed back by GitLab, and never carried in
///         an exception message.
///     </para>
///     <para>
///         The project endpoints (<c>/projects/:id/managed_licenses</c>) are software licence policies -
///         part of licence compliance scanning, and nothing to do with activating GitLab.
///     </para>
/// </summary>
public interface ILicensesClient
{
    /// <summary>Gets the licence currently activating the instance, including its billable user count.</summary>
    Task<GitLabLicense> GetCurrentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a licence key and activates it. The key in
    ///     <see cref="CreateLicenseRequest.License" /> is as sensitive as a token - see the remarks on that
    ///     record.
    /// </summary>
    Task<GitLabLicense> CreateAsync(CreateLicenseRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads the usage data for the active licence. The body is a report file rather than JSON, so
    ///     the caller owns the returned <see cref="GitLabFileResponse" /> and must
    ///     <see langword="await" /> <see langword="using" /> it.
    /// </summary>
    Task<GitLabFileResponse> DownloadUsageExportAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every licence the instance has ever had. These entries omit
    ///     <see cref="GitLabLicense.ActiveUsers" />; <see cref="GetAsync" /> fills it in.
    /// </summary>
    IAsyncEnumerable<GitLabLicense> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets one licence by its numeric id, including its billable user count.</summary>
    Task<GitLabLicense> GetAsync(long licenseId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a licence. Deleting the active licence deactivates Enterprise Edition features on the
    ///     instance.
    /// </summary>
    Task DeleteAsync(long licenseId, CancellationToken cancellationToken = default);

    /// <summary>Triggers a recalculation of the licence's billable user count.</summary>
    Task<GitLabLicenseRefreshResult> RefreshBillableUsersAsync(long licenseId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams a project's software licence policies.</summary>
    IAsyncEnumerable<GitLabManagedLicense> ListManagedAsync(ProjectId projectId,
        ManagedLicenseListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one software licence policy. GitLab types the identifier as a string; licence names legally
    ///     contain dots and slashes, and the route builder percent-encodes it, so pass it raw.
    /// </summary>
    Task<GitLabManagedLicense> GetManagedAsync(ProjectId projectId, string managedLicenseId,
        CancellationToken cancellationToken = default);

    /// <summary>Records a verdict for a licence on a project.</summary>
    Task<GitLabManagedLicense> CreateManagedAsync(ProjectId projectId, CreateManagedLicenseRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Changes an existing software licence policy's name or verdict.</summary>
    Task<GitLabManagedLicense> UpdateManagedAsync(ProjectId projectId, string managedLicenseId,
        UpdateManagedLicenseRequest request, CancellationToken cancellationToken = default);

    /// <summary>Removes a software licence policy from a project.</summary>
    Task DeleteManagedAsync(ProjectId projectId, string managedLicenseId,
        CancellationToken cancellationToken = default);
}