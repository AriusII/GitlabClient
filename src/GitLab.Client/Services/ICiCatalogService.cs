using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for CI Catalog, sitting between the public <c>ICiCatalogClient</c>
///     controller and <c>ICiCatalogRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ICiCatalogService
{
    Task<GitLabCiCatalogPublishResult> PublishAsync(ProjectId projectId, CiCatalogPublishRequest request,
        CancellationToken cancellationToken = default);
}