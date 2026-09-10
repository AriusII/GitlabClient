namespace GitLab.Client.Models;

/// <summary>
///     An SSH certificate authority registered on a group
///     (<c>/groups/:id/ssh_certificates</c>). GitLab groups this with the user SSH keys under its
///     "Keys" API area, but the two are different things: this is the CA public key a group trusts to
///     sign its members' short-lived certificates, not a key belonging to a person.
/// </summary>
public sealed record GitLabSshCertificate
{
    public required long Id { get; init; }

    /// <summary>The human-readable label for the certificate authority.</summary>
    public required string Title { get; init; }

    /// <summary>The certificate authority's public key, in <c>authorized_keys</c> form.</summary>
    public required string Key { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}