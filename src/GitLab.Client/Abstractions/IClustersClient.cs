using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's deprecated certificate-based Kubernetes Clusters API. For new Kubernetes
///     integrations, prefer <see cref="IClusterAgentsClient" /> and the GitLab agent for Kubernetes.
///     <para>
///         These endpoints remain part of GitLab 19.x's REST API, so this client retains complete typed
///         access for existing instance, group, and project cluster associations without conflating them
///         with agent registrations.
///     </para>
/// </summary>
public interface IClustersClient
{
    /// <summary>Streams every certificate-based cluster configured for the GitLab instance. Administrators only.</summary>
    IAsyncEnumerable<GitLabCluster> ListForInstanceAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets one certificate-based instance cluster. Administrators only.</summary>
    Task<GitLabCluster> GetForInstanceAsync(long clusterId, CancellationToken cancellationToken = default);

    /// <summary>Adds an existing Kubernetes cluster to the GitLab instance. Administrators only.</summary>
    Task<GitLabCluster> CreateForInstanceAsync(CreateClusterRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Partially updates one certificate-based instance cluster. Administrators only.</summary>
    Task<GitLabCluster> UpdateForInstanceAsync(long clusterId, UpdateClusterRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes an instance cluster association. GitLab does not delete the resources still present in
    ///     the Kubernetes cluster.
    /// </summary>
    Task<GitLabCluster> DeleteForInstanceAsync(long clusterId, CancellationToken cancellationToken = default);

    /// <summary>Streams every certificate-based cluster associated with a group.</summary>
    IAsyncEnumerable<GitLabGroupCluster> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one certificate-based cluster associated with a group.</summary>
    Task<GitLabGroupCluster> GetForGroupAsync(GroupId groupId, long clusterId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds an existing Kubernetes cluster to a group.</summary>
    Task<GitLabGroupCluster> CreateForGroupAsync(GroupId groupId, CreateClusterRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Partially updates one certificate-based group cluster.</summary>
    Task<GitLabGroupCluster> UpdateForGroupAsync(GroupId groupId, long clusterId, UpdateClusterRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a group cluster association without deleting the resources still present in Kubernetes.
    /// </summary>
    Task<GitLabGroupCluster> DeleteForGroupAsync(GroupId groupId, long clusterId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every certificate-based cluster associated with a project.</summary>
    IAsyncEnumerable<GitLabCluster> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one certificate-based cluster associated with a project.</summary>
    Task<GitLabProjectCluster> GetForProjectAsync(ProjectId projectId, long clusterId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds an existing Kubernetes cluster to a project.</summary>
    Task<GitLabProjectCluster> CreateForProjectAsync(ProjectId projectId, CreateClusterRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Partially updates one certificate-based project cluster.</summary>
    Task<GitLabProjectCluster> UpdateForProjectAsync(ProjectId projectId, long clusterId,
        UpdateClusterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a project cluster association without deleting the resources still present in Kubernetes.
    /// </summary>
    Task<GitLabProjectCluster> DeleteForProjectAsync(ProjectId projectId, long clusterId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds certificate-based clusters reachable through a group's hierarchy
    ///     (<c>GET /discover-cert-based-clusters?group_id=:id</c>). Introduced in GitLab 17.9.
    /// </summary>
    Task<GitLabDiscoveredCertificateBasedClusters> DiscoverCertificateBasedAsync(long groupId,
        CancellationToken cancellationToken = default);
}