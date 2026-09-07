namespace GitLab.Client.Models;

/// <summary>
///     An SSH deploy key, which grants repository access to CI jobs and external systems without a user
///     account (<c>/projects/:id/deploy_keys</c>).
///     <para>
///         Models both <c>APIEntitiesDeployKey</c> (the instance-wide and update shapes) and
///         <c>APIEntitiesDeployKeysProject</c> (the project list/get/add shapes). The only difference is
///         <see cref="CanPush" />, which is why it is nullable: a key returned by
///         <c>UpdateAsync</c> carries no push flag.
///     </para>
/// </summary>
public sealed record GitLabDeployKey
{
    public required long Id { get; init; }

    public required string Title { get; init; }

    /// <summary>The public key material, in <c>authorized_keys</c> form.</summary>
    public required string Key { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    ///     When the key stops being accepted. A full timestamp here, unlike a member's date-only expiry.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    public DateTimeOffset? LastUsedAt { get; init; }

    /// <summary>Either "auth" or "auth_and_signing".</summary>
    public string? UsageType { get; init; }

    public string? Fingerprint { get; init; }

    public string? FingerprintSha256 { get; init; }

    /// <summary>
    ///     Whether the key may push to this project's repository. Returned by the project-scoped list, get and
    ///     add endpoints only.
    /// </summary>
    public bool? CanPush { get; init; }

    /// <summary>
    ///     The projects this key can push to, beyond the one it was created on. Returned only by the
    ///     instance-wide <c>POST /deploy_keys</c> (administrator-only) response.
    /// </summary>
    public GitLabProjectIdentity? ProjectsWithWriteAccess { get; init; }

    /// <summary>
    ///     The projects this key can read but not push to. Returned only by the instance-wide
    ///     <c>POST /deploy_keys</c> (administrator-only) response.
    /// </summary>
    public GitLabProjectIdentity? ProjectsWithReadonlyAccess { get; init; }
}