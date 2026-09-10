using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

/// <summary>Direct transport mapping for GitLab's deprecated certificate-based Clusters API.</summary>
internal sealed class ClustersClient(IGitLabApiConnection connection) : IClustersClient
{
    public IAsyncEnumerable<GitLabCluster> ListForInstanceAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            InstanceRoute().Build(),
            GitLabJsonContext.Default.GitLabClusterArray,
            cancellationToken);
    }

    public Task<GitLabCluster> GetForInstanceAsync(long clusterId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            InstanceRoute().Segment(clusterId).Build(),
            GitLabJsonContext.Default.GitLabCluster,
            cancellationToken);
    }

    public Task<GitLabCluster> CreateForInstanceAsync(CreateClusterRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            InstanceRoute().Literal("add").Build(),
            request,
            GitLabJsonContext.Default.CreateClusterRequest,
            GitLabJsonContext.Default.GitLabCluster,
            cancellationToken);
    }

    public Task<GitLabCluster> UpdateForInstanceAsync(long clusterId, UpdateClusterRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            InstanceRoute().Segment(clusterId).Build(),
            request,
            GitLabJsonContext.Default.UpdateClusterRequest,
            GitLabJsonContext.Default.GitLabCluster,
            cancellationToken);
    }

    public Task<GitLabCluster> DeleteForInstanceAsync(long clusterId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            InstanceRoute().Segment(clusterId).Build(),
            GitLabJsonContext.Default.GitLabCluster,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroupCluster> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GroupRoute(groupId).Build(),
            GitLabJsonContext.Default.GitLabGroupClusterArray,
            cancellationToken);
    }

    public Task<GitLabGroupCluster> GetForGroupAsync(GroupId groupId, long clusterId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupRoute(groupId).Segment(clusterId).Build(),
            GitLabJsonContext.Default.GitLabGroupCluster,
            cancellationToken);
    }

    public Task<GitLabGroupCluster> CreateForGroupAsync(GroupId groupId, CreateClusterRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GroupRoute(groupId).Literal("user").Build(),
            request,
            GitLabJsonContext.Default.CreateClusterRequest,
            GitLabJsonContext.Default.GitLabGroupCluster,
            cancellationToken);
    }

    public Task<GitLabGroupCluster> UpdateForGroupAsync(GroupId groupId, long clusterId,
        UpdateClusterRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GroupRoute(groupId).Segment(clusterId).Build(),
            request,
            GitLabJsonContext.Default.UpdateClusterRequest,
            GitLabJsonContext.Default.GitLabGroupCluster,
            cancellationToken);
    }

    public Task<GitLabGroupCluster> DeleteForGroupAsync(GroupId groupId, long clusterId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GroupRoute(groupId).Segment(clusterId).Build(),
            GitLabJsonContext.Default.GitLabGroupCluster,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabCluster> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectRoute(projectId).Build(),
            GitLabJsonContext.Default.GitLabClusterArray,
            cancellationToken);
    }

    public Task<GitLabProjectCluster> GetForProjectAsync(ProjectId projectId, long clusterId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectRoute(projectId).Segment(clusterId).Build(),
            GitLabJsonContext.Default.GitLabProjectCluster,
            cancellationToken);
    }

    public Task<GitLabProjectCluster> CreateForProjectAsync(ProjectId projectId, CreateClusterRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectRoute(projectId).Literal("user").Build(),
            request,
            GitLabJsonContext.Default.CreateClusterRequest,
            GitLabJsonContext.Default.GitLabProjectCluster,
            cancellationToken);
    }

    public Task<GitLabProjectCluster> UpdateForProjectAsync(ProjectId projectId, long clusterId,
        UpdateClusterRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectRoute(projectId).Segment(clusterId).Build(),
            request,
            GitLabJsonContext.Default.UpdateClusterRequest,
            GitLabJsonContext.Default.GitLabProjectCluster,
            cancellationToken);
    }

    public Task<GitLabProjectCluster> DeleteForProjectAsync(ProjectId projectId, long clusterId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ProjectRoute(projectId).Segment(clusterId).Build(),
            GitLabJsonContext.Default.GitLabProjectCluster,
            cancellationToken);
    }

    public Task<GitLabDiscoveredCertificateBasedClusters> DiscoverCertificateBasedAsync(long groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("discover-cert-based-clusters").Query("group_id", groupId).Build(),
            GitLabJsonContext.Default.GitLabDiscoveredCertificateBasedClusters,
            cancellationToken);
    }

    private static GitLabRouteBuilder InstanceRoute()
    {
        return GitLabRouteBuilder.Create("admin").Literal("clusters");
    }

    private static GitLabRouteBuilder GroupRoute(GroupId groupId)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("clusters");
    }

    private static GitLabRouteBuilder ProjectRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("clusters");
    }
}