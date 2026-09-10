namespace GitLab.Client.Models;

/// <summary>
///     Kubernetes platform details embedded in a certificate-based cluster response
///     (<c>APIEntitiesPlatformKubernetes</c>).
/// </summary>
public sealed record GitLabClusterKubernetesPlatform
{
    /// <summary>The Kubernetes API endpoint configured in GitLab.</summary>
    public Uri? ApiUrl { get; init; }

    public string? Namespace { get; init; }

    /// <summary>GitLab's reported authorization mode, such as <c>rbac</c>.</summary>
    public string? AuthorizationType { get; init; }

    /// <summary>The PEM CA certificate GitLab uses when validating the Kubernetes API endpoint.</summary>
    public string? CaCert { get; init; }
}