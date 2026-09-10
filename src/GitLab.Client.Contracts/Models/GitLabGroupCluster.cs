namespace GitLab.Client.Models;

/// <summary>
///     A certificate-based Kubernetes cluster attached to a group
///     (<c>APIEntitiesClusterGroup</c>).
/// </summary>
public sealed record GitLabGroupCluster : GitLabCluster
{
    /// <summary>The group that owns this cluster association.</summary>
    public GitLabBasicGroupDetails? Group { get; init; }
}