using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Deployments resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IDeploymentsService), typeof(IDeploymentsClient))]
internal interface IDeploymentsRepository
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