namespace GitLab.Client.Models;

/// <summary>
///     A project's security settings, as returned by <c>GET</c> and
///     <c>
///         PUT
///         /projects/:id/security_settings
///     </c>
///     .
/// </summary>
/// <remarks>
///     The vendored spec declares the response of both operations as a bare "200 OK" with no schema, so
///     the members below are modelled from the settings the request body can write plus the identifying
///     fields GitLab returns alongside them. Every member is therefore nullable: a field this record does
///     not know about is skipped, and one GitLab stops sending simply reads back as null instead of
///     failing the whole response.
/// </remarks>
public sealed record GitLabProjectSecuritySettings
{
    /// <summary>The project these settings belong to.</summary>
    public long? ProjectId { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Whether pushes containing secrets are rejected.</summary>
    public bool? SecretPushProtectionEnabled { get; init; }

    /// <summary>The pre-receive spelling of <see cref="SecretPushProtectionEnabled" />.</summary>
    public bool? PreReceiveSecretDetectionEnabled { get; init; }

    /// <summary>Whether dependency-path graphs record one path per dependency rather than every path.</summary>
    public bool? FastDependencyPathsEnabled { get; init; }

    /// <summary>Whether leaked-credential validity checking is on.</summary>
    public bool? ValidityChecksEnabled { get; init; }
}