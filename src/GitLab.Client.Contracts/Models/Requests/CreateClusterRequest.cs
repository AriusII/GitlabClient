namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body used to add an existing certificate-based Kubernetes cluster to an instance, group,
///     or project.
/// </summary>
public sealed record CreateClusterRequest
{
    /// <summary>The cluster's display name.</summary>
    public required string Name { get; init; }

    /// <summary>Whether GitLab should enable the newly associated cluster. GitLab defaults to <see langword="true" />.</summary>
    public bool? Enabled { get; init; }

    public string? Domain { get; init; }

    /// <summary>The environment scope GitLab associates with the cluster. GitLab defaults to <c>*</c>.</summary>
    public string? EnvironmentScope { get; init; }

    public bool? NamespacePerEnvironment { get; init; }

    public long? ManagementProjectId { get; init; }

    public bool? Managed { get; init; }

    public required CreateClusterKubernetesAttributes PlatformKubernetesAttributes { get; init; }
}