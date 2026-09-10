namespace GitLab.Client.Models;

/// <summary>
///     The instance-wide key GitLab signs web-UI and API-created commits with
///     (<c>GET /web_commits/public_key</c>). Introduced in GitLab 17.4.
/// </summary>
public sealed record GitLabWebCommitsPublicKey
{
    /// <summary>The public key in <c>authorized_keys</c> form, for example <c>ssh-ed25519 AAAA...</c>.</summary>
    public required string PublicKey { get; init; }
}