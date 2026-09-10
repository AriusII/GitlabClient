namespace GitLab.Client.Models.Requests;

/// <summary>
///     Required Kubernetes connection data when adding an existing, certificate-based Kubernetes cluster.
/// </summary>
public sealed record CreateClusterKubernetesAttributes
{
    /// <summary>The Kubernetes API endpoint GitLab should connect to.</summary>
    public required Uri ApiUrl { get; init; }

    /// <summary>The Kubernetes API credential. Never log this request.</summary>
    public required string Token { get; init; }

    /// <summary>The PEM CA certificate required for a self-signed Kubernetes API certificate.</summary>
    public string? CaCert { get; init; }

    /// <summary>The project or group namespace associated with this cluster.</summary>
    public string? Namespace { get; init; }

    /// <summary>The authorization mode. GitLab defaults this to RBAC when it is omitted.</summary>
    public GitLabClusterAuthorizationType? AuthorizationType { get; init; }

    /// <summary>Redacts the Kubernetes credential from record formatting.</summary>
    public override string ToString()
    {
        return $"CreateClusterKubernetesAttributes {{ ApiUrl = {ApiUrl}, Token = <redacted> }}";
    }
}