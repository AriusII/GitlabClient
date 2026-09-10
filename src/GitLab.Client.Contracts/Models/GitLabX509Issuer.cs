namespace GitLab.Client.Models;

/// <summary>The certificate authority that issued a <see cref="GitLabX509Certificate" />.</summary>
public sealed record GitLabX509Issuer
{
    public long? Id { get; init; }

    /// <summary>The issuer's distinguished name, for example <c>CN=PKI,OU=Example,O=World</c>.</summary>
    public string? Subject { get; init; }

    /// <summary>The colon-separated hex fingerprint of the issuer's public key.</summary>
    public string? SubjectKeyIdentifier { get; init; }

    /// <summary>Where the issuer publishes its certificate revocation list.</summary>
    public Uri? CrlUrl { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}