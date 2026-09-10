namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>PUT /projects/:id/security_settings</c>.</summary>
public sealed record UpdateProjectSecuritySettingsRequest
{
    /// <summary>Whether pushes containing secrets are rejected.</summary>
    public bool? SecretPushProtectionEnabled { get; init; }

    /// <summary>The pre-receive spelling of <see cref="SecretPushProtectionEnabled" />.</summary>
    public bool? PreReceiveSecretDetectionEnabled { get; init; }

    /// <summary>
    ///     Whether dependency-path graphs record one path per dependency instead of every possible path,
    ///     which is faster but less complete.
    /// </summary>
    public bool? FastDependencyPathsEnabled { get; init; }
}