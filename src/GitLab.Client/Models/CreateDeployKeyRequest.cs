namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/deploy_keys</c>, and reused for the instance-wide, administrator-only
///     <c>POST /deploy_keys</c> (which does not declare <see cref="CanPush" /> - GitLab silently ignores it there,
///     as it does for any parameter an endpoint does not declare).
/// </summary>
public sealed record CreateDeployKeyRequest
{
    /// <summary>The public key material, in <c>authorized_keys</c> form.</summary>
    public required string Key { get; init; }

    public required string Title { get; init; }

    /// <summary>
    ///     Whether the key may push to the project's repository. GitLab defaults this to false. Honoured only
    ///     by the project-scoped endpoint.
    /// </summary>
    public bool? CanPush { get; init; }

    /// <summary>An optional expiry, sent as an ISO 8601 timestamp.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }
}