namespace GitLab.Client.Models;

/// <summary>
///     The X.509 certificate behind an X509-signed commit or tag.
/// </summary>
/// <remarks>
///     GitLab also sends <c>serial_number</c>, which is deliberately not modelled: it is a full 20-byte
///     certificate serial rendered as a bare JSON number (values past 10^38 are routine), so no .NET
///     integral type can hold it and binding it to one would turn a healthy response into a
///     <see cref="System.Text.Json.JsonException" />. Unmapped members are skipped, so its presence costs
///     nothing.
/// </remarks>
public sealed record GitLabX509Certificate
{
    public long? Id { get; init; }

    /// <summary>The certificate's distinguished name, for example <c>CN=gitlab@example.org,O=World</c>.</summary>
    public string? Subject { get; init; }

    /// <summary>The colon-separated hex fingerprint of the certificate's public key.</summary>
    public string? SubjectKeyIdentifier { get; init; }

    /// <summary>The email address the certificate is bound to.</summary>
    public string? Email { get; init; }

    /// <summary>GitLab's revocation view of the certificate, <c>good</c> or <c>revoked</c>.</summary>
    public string? CertificateStatus { get; init; }

    /// <summary>The issuing certificate authority.</summary>
    public GitLabX509Issuer? X509Issuer { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}