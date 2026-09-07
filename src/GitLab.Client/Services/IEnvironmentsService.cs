using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Environments, sitting between the public <c>IEnvironmentsClient</c>
///     controller and <c>IEnvironmentsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IEnvironmentsService
{
    IAsyncEnumerable<GitLabEnvironment> ListAsync(ProjectId projectId, EnvironmentListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabEnvironment> GetAsync(ProjectId projectId, long environmentId,
        CancellationToken cancellationToken = default);

    Task<GitLabEnvironment> CreateAsync(ProjectId projectId, CreateEnvironmentRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabEnvironment> UpdateAsync(ProjectId projectId, long environmentId, UpdateEnvironmentRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabEnvironment> StopAsync(ProjectId projectId, long environmentId,
        CancellationToken cancellationToken = default);

    Task<GitLabEnvironment> StopAsync(ProjectId projectId, long environmentId, bool force,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long environmentId, CancellationToken cancellationToken = default);

    Task StopStaleAsync(ProjectId projectId, StopStaleEnvironmentsRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteReviewAppsAsync(ProjectId projectId, ReviewAppDeletionOptions? options = null,
        CancellationToken cancellationToken = default);
}