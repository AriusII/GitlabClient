namespace GitLab.Client.Models;

/// <summary>
///     Google Cloud Storage access for an offline transfer through Application Default Credentials -
///     the one configuration that carries no secret, because the credential comes from the instance's own
///     environment.
/// </summary>
/// <remarks>
///     Available on GitLab Self-Managed and GitLab Dedicated only, must be enabled by an administrator,
///     is usable only by administrators, and the bucket name must start with
///     <c>gitlab-offline-transfer-</c>.
/// </remarks>
public sealed record OfflineTransferGcsAdcConfiguration
{
    /// <summary>The Google Cloud project ID.</summary>
    public required string GoogleProject { get; init; }
}