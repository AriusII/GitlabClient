namespace GitLab.Client.Models.Responses;

/// <summary>
///     The SSH public key exposed for a remote mirror by
///     <c>GET /projects/:id/remote_mirrors/:mirror_id/public_key</c>.
/// </summary>
/// <remarks>
///     This response was introduced in GitLab 17.9. Its schema is missing from the pinned 19.4 OpenAPI
///     document even though GitLab documents the <c>public_key</c> response member. The endpoint client
///     therefore maps the already source-generated <see cref="System.Text.Json.JsonElement" /> payload into
///     this strongly typed response without using reflection-based serialization.
/// </remarks>
public sealed record GitLabRemoteMirrorPublicKey
{
    /// <summary>The key in OpenSSH <c>authorized_keys</c> form, for example <c>ssh-rsa AAAA...</c>.</summary>
    public required string PublicKey { get; init; }
}