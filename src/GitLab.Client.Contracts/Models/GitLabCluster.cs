namespace GitLab.Client.Models;

/// <summary>
///     The common projection for GitLab's deprecated certificate-based Kubernetes clusters
///     (<c>APIEntitiesCluster</c>). GitLab 19.x continues to expose these resources, although new
///     integrations should normally use <see cref="GitLabClusterAgent" /> instead.
/// </summary>
/// <remarks>
///     This type is intentionally separate from <see cref="GitLabClusterAgent" />. A certificate-based
///     cluster stores GitLab's connection credentials and lifecycle settings, whereas an agent is a
///     registration for an in-cluster process which authenticates back to GitLab.
/// </remarks>
public record GitLabCluster
{
    public long? Id { get; init; }

    public string? Name { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? Domain { get; init; }

    public bool? Enabled { get; init; }

    public bool? Managed { get; init; }

    public string? ProviderType { get; init; }

    public string? PlatformType { get; init; }

    public string? EnvironmentScope { get; init; }

    public string? ClusterType { get; init; }

    public bool? NamespacePerEnvironment { get; init; }

    public GitLabBasicUser? User { get; init; }

    public GitLabClusterKubernetesPlatform? PlatformKubernetes { get; init; }

    public GitLabClusterGcpProvider? ProviderGcp { get; init; }

    public GitLabProjectIdentity? ManagementProject { get; init; }
}