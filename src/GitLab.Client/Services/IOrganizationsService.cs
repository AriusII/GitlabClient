using GitLab.Client.Abstractions;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Organizations, sitting between the public
///     <c>IOrganizationsClient</c> controller and <c>IOrganizationsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once this resource
///     needs more than pass-through.
/// </summary>
internal interface IOrganizationsService
{
    Task<GitLabOrganization> CreateAsync(CreateOrganizationRequest request, GitLabFileUpload? avatar = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long organizationId, CancellationToken cancellationToken = default);
}