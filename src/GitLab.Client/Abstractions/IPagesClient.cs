using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab Pages API area - a project's Pages settings (<c>/projects/:id/pages</c>), its
///     custom domains (<c>/projects/:id/pages/domains</c>) and the instance-wide domain listing
///     (<c>/pages/domains</c>).
///     <para>
///         GitLab Pages must be enabled on the instance for any of this to answer; where it is not, GitLab
///         returns <c>404</c> rather than <c>403</c>, so a
///         <see cref="Exceptions.GitLabNotFoundException" /> here does not necessarily mean the project or
///         domain is missing.
///     </para>
///     <para>
///         Custom domains are free text containing dots, and a wildcard domain contains <c>*</c>. Pass them
///         raw - they are percent-encoded for you.
///     </para>
///     <para>
///         Certificates: the create and update requests carry a private key, and no response on this
///         surface ever returns one. Treat <see cref="CreatePagesDomainRequest.Key" /> and
///         <see cref="UpdatePagesDomainRequest.Key" /> as secrets - do not log those request records.
///     </para>
/// </summary>
public interface IPagesClient
{
    /// <summary>
    ///     Retrieves a project's Pages settings - where the site is served, whether HTTPS is forced, which
    ///     domain is canonical, and what is currently deployed. Requires the Maintainer or Owner role.
    /// </summary>
    Task<GitLabPagesSettings> GetSettingsAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a project's Pages settings. Only the members set on the request are sent; anything left
    ///     null keeps its current value. Requires the Maintainer or Owner role.
    /// </summary>
    Task<GitLabPagesSettings> UpdateSettingsAsync(ProjectId projectId, UpdatePagesSettingsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Unpublishes a project's Pages site, deleting what is deployed. The project's Pages configuration
    ///     and custom domains survive; a later pipeline republishes. Requires the Maintainer or Owner role.
    /// </summary>
    Task UnpublishAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether the caller may view a project's Pages site
    ///     (<c>GET /projects/:id/pages_access</c>).
    ///     <para>
    ///         The answer is the status code alone - GitLab replies <c>200</c> with an empty body - so this
    ///         returns nothing and throws instead when access is refused. Completing normally means yes.
    ///     </para>
    /// </summary>
    /// <exception cref="Exceptions.GitLabForbiddenException">The caller may not view the site.</exception>
    /// <exception cref="Exceptions.GitLabNotFoundException">
    ///     The project does not exist, is invisible to the caller, or has no Pages site.
    /// </exception>
    Task CheckAccessAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every Pages domain on the whole instance. Requires administrator access.
    /// </summary>
    /// <param name="domain">Restricts the result to one hostname, or null for all of them.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <remarks>
    ///     Yields <see cref="GitLabPagesDomainSummary" />, not <see cref="GitLabPagesDomain" />: the
    ///     instance-wide view names each domain's owning project and reports only whether its certificate
    ///     has expired.
    /// </remarks>
    IAsyncEnumerable<GitLabPagesDomainSummary> ListAllDomainsAsync(string? domain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every Pages domain configured on one project.</summary>
    IAsyncEnumerable<GitLabPagesDomain> ListDomainsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one Pages domain of a project by hostname, passed raw.</summary>
    Task<GitLabPagesDomain> GetDomainAsync(ProjectId projectId, string domain,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a custom domain to a project's Pages site. Supply a certificate and key, or set
    ///     <see cref="CreatePagesDomainRequest.AutoSslEnabled" /> to have GitLab obtain one from Let's
    ///     Encrypt once the domain verifies.
    /// </summary>
    Task<GitLabPagesDomain> CreateDomainAsync(ProjectId projectId, CreatePagesDomainRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates the certificate on an existing Pages domain, or switches it to Let's Encrypt. The
    ///     hostname itself cannot be changed - delete the domain and create it again.
    /// </summary>
    Task<GitLabPagesDomain> UpdateDomainAsync(ProjectId projectId, string domain, UpdatePagesDomainRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a custom domain from a project's Pages site.</summary>
    Task DeleteDomainAsync(ProjectId projectId, string domain, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asks GitLab to re-check the domain's <c>_gitlab-pages-verification-code</c> TXT record now,
    ///     rather than waiting for the next scheduled check.
    /// </summary>
    /// <returns>
    ///     The domain as it stands after the check - read <see cref="GitLabPagesDomain.Verified" /> to see
    ///     whether it succeeded. A failed check is not an error status.
    /// </returns>
    Task<GitLabPagesDomain> VerifyDomainAsync(ProjectId projectId, string domain,
        CancellationToken cancellationToken = default);
}