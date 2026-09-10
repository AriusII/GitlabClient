namespace GitLab.Client.Models.Requests;

/// <summary>
///     Partial update for a certificate-based Kubernetes cluster. Omitted members are left unchanged by
///     GitLab.
/// </summary>
public sealed record UpdateClusterRequest
{
    public string? Name { get; init; }

    public bool? Enabled { get; init; }

    public string? Domain { get; init; }

    public string? EnvironmentScope { get; init; }

    public bool? NamespacePerEnvironment { get; init; }

    public long? ManagementProjectId { get; init; }

    public bool? Managed { get; init; }

    public UpdateClusterKubernetesAttributes? PlatformKubernetesAttributes { get; init; }
}