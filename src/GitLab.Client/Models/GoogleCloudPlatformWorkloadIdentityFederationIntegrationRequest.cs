namespace GitLab.Client.Models;

/// <summary>
///     Typed settings for the <c>google-cloud-platform-workload-identity-federation</c> integration -
///     see <see cref="GitLabIntegrationSlug.GoogleCloudPlatformWorkloadIdentityFederation" />. Keyless
///     authentication to Google Cloud from GitLab CI/CD, configured entirely by pool/provider
///     identifiers rather than a stored secret.
/// </summary>
public sealed record GoogleCloudPlatformWorkloadIdentityFederationIntegrationRequest
{
    /// <summary>Google Cloud project ID for the Workload Identity Federation.</summary>
    public required string WorkloadIdentityFederationProjectId { get; init; }

    /// <summary>Google Cloud project number for the Workload Identity Federation.</summary>
    public required string WorkloadIdentityFederationProjectNumber { get; init; }

    /// <summary>ID of the Workload Identity Pool.</summary>
    public required string WorkloadIdentityPoolId { get; init; }

    /// <summary>ID of the Workload Identity Pool provider.</summary>
    public required string WorkloadIdentityPoolProviderId { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <see langword="false" />.</summary>
    public bool? UseInheritedSettings { get; init; }
}