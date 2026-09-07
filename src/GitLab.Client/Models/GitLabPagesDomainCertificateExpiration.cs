namespace GitLab.Client.Models;

/// <summary>
///     The expiry view of a Pages domain's TLS certificate, which is all the instance-wide domain listing
///     (<c>GET /pages/domains</c>) exposes - it reports whether each certificate has lapsed without
///     shipping the certificate itself.
/// </summary>
public sealed record GitLabPagesDomainCertificateExpiration
{
    /// <summary>Whether the certificate is past its expiry date.</summary>
    public bool? Expired { get; init; }

    /// <summary>When the certificate expires.</summary>
    public DateTimeOffset? Expiration { get; init; }
}