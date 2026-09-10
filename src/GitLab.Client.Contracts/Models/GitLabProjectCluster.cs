namespace GitLab.Client.Models;

/// <summary>
///     A certificate-based Kubernetes cluster attached to a project
///     (<c>APIEntitiesClusterProject</c>).
/// </summary>
public sealed record GitLabProjectCluster : GitLabCluster
{
    /// <summary>The project that owns this cluster association.</summary>
    public GitLabBasicProjectDetails? Project { get; init; }
}