namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the Apple App Store integration (<c>PUT /projects/:id/integrations/apple-app-store</c>).
/// </summary>
public sealed record AppleAppStoreIntegrationRequest
{
    /// <summary>Apple App Store Connect issuer ID.</summary>
    public required string AppStoreIssuerId { get; init; }

    /// <summary>Apple App Store Connect key ID.</summary>
    public required string AppStoreKeyId { get; init; }

    /// <summary>Apple App Store Connect private-key file name.</summary>
    public required string AppStorePrivateKeyFileName { get; init; }

    /// <summary>Apple App Store Connect private key.</summary>
    public required string AppStorePrivateKey { get; init; }

    /// <summary>Sets variables on protected branches and tags only.</summary>
    public bool? AppStoreProtectedRefs { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}