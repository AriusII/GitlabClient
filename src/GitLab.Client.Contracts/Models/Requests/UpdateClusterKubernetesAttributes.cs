namespace GitLab.Client.Models.Requests;

/// <summary>
///     Kubernetes connection fields to change on an existing certificate-based cluster. Every field is
///     optional so callers update only the settings GitLab supports changing.
/// </summary>
public sealed record UpdateClusterKubernetesAttributes
{
    public Uri? ApiUrl { get; init; }

    /// <summary>A replacement Kubernetes API credential. Never log this request.</summary>
    public string? Token { get; init; }

    public string? CaCert { get; init; }

    public string? Namespace { get; init; }

    /// <summary>Redacts the Kubernetes credential from record formatting.</summary>
    public override string ToString()
    {
        return $"UpdateClusterKubernetesAttributes {{ ApiUrl = {ApiUrl}, Token = <redacted> }}";
    }
}