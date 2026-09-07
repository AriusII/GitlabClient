using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class DeploymentsRepository(IGitLabApiConnection connection) : IDeploymentsRepository
{
    public IAsyncEnumerable<GitLabDeployment> ListAsync(ProjectId projectId, DeploymentListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("deployments")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabDeploymentArray,
            cancellationToken);
    }

    public Task<GitLabDeployment> GetAsync(ProjectId projectId, long deploymentId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("deployments")
                .Segment(deploymentId)
                .Build(),
            GitLabJsonContext.Default.GitLabDeployment,
            cancellationToken);
    }

    public Task<GitLabDeployment> CreateAsync(ProjectId projectId, CreateDeploymentRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("deployments")
                .Build(),
            request,
            GitLabJsonContext.Default.CreateDeploymentRequest,
            GitLabJsonContext.Default.GitLabDeployment,
            cancellationToken);
    }

    public Task<GitLabDeployment> UpdateAsync(ProjectId projectId, long deploymentId, UpdateDeploymentRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("deployments")
                .Segment(deploymentId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateDeploymentRequest,
            GitLabJsonContext.Default.GitLabDeployment,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long deploymentId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("deployments")
                .Segment(deploymentId)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabDeploymentApproval> ApproveAsync(ProjectId projectId, long deploymentId,
        ApproveDeploymentRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("deployments")
                .Segment(deploymentId)
                .Literal("approval")
                .Build(),
            request,
            GitLabJsonContext.Default.ApproveDeploymentRequest,
            GitLabJsonContext.Default.GitLabDeploymentApproval,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabMergeRequest> ListMergeRequestsAsync(ProjectId projectId, long deploymentId,
        MergeRequestListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("deployments")
                .Segment(deploymentId)
                .Literal("merge_requests")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabMergeRequestArray,
            cancellationToken);
    }
}