using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "CI catalog" API area (<c>/projects/:id/catalog/publish</c>): publishes a
///     release of a component project as a new version to the CI/CD catalog.
/// </summary>
public interface ICiCatalogClient
{
    /// <summary>
    ///     Publishes a new component project release as a version to the CI/CD catalog
    ///     (<c>POST /projects/:id/catalog/publish</c>).
    /// </summary>
    Task<GitLabCiCatalogPublishResult> PublishAsync(ProjectId projectId, CiCatalogPublishRequest request,
        CancellationToken cancellationToken = default);
}