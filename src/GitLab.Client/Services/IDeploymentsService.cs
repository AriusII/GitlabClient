using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Deployments, sitting between the public <c>IDeploymentsClient</c>
///     controller and <c>IDeploymentsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IDeploymentsService
{
    IAsyncEnumerable<GitLabDeployment> ListAsync(ProjectId projectId, DeploymentListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabDeployment> GetAsync(ProjectId projectId, long deploymentId,
        CancellationToken cancellationToken = default);

    Task<GitLabDeployment> CreateAsync(ProjectId projectId, CreateDeploymentRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabDeployment> UpdateAsync(ProjectId projectId, long deploymentId, UpdateDeploymentRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long deploymentId, CancellationToken cancellationToken = default);

    Task<GitLabDeploymentApproval> ApproveAsync(ProjectId projectId, long deploymentId,
        ApproveDeploymentRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequest> ListMergeRequestsAsync(ProjectId projectId, long deploymentId,
        MergeRequestListOptions? options = null, CancellationToken cancellationToken = default);
}