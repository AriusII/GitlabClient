using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Deployments" API area (<c>/projects/:id/deployments</c>).</summary>
public interface IDeploymentsClient
{
    /// <summary>Streams every deployment in the project, following the pagination links.</summary>
    IAsyncEnumerable<GitLabDeployment> ListAsync(ProjectId projectId, DeploymentListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabDeployment> GetAsync(ProjectId projectId, long deploymentId,
        CancellationToken cancellationToken = default);

    /// <summary>Records a deployment - typically one performed outside GitLab CI/CD - against an environment.</summary>
    Task<GitLabDeployment> CreateAsync(ProjectId projectId, CreateDeploymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Moves an existing deployment to a new status.</summary>
    Task<GitLabDeployment> UpdateAsync(ProjectId projectId, long deploymentId, UpdateDeploymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a deployment. GitLab permits this only for a finished deployment that is not the current one
    ///     for its environment; anything else comes back as a 403 or 400.
    /// </summary>
    Task DeleteAsync(ProjectId projectId, long deploymentId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Approves or rejects a deployment blocked on a protected environment's approval rules. Returns the
    ///     recorded approval, not the updated deployment.
    /// </summary>
    Task<GitLabDeploymentApproval> ApproveAsync(ProjectId projectId, long deploymentId,
        ApproveDeploymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>Streams every merge request GitLab associates with this deployment.</summary>
    IAsyncEnumerable<GitLabMergeRequest> ListMergeRequestsAsync(ProjectId projectId, long deploymentId,
        MergeRequestListOptions? options = null, CancellationToken cancellationToken = default);
}