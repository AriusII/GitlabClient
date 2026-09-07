namespace GitLab.Client.Models;

/// <summary>
///     A Pages custom domain as reported by the instance-wide administrator listing
///     (<c>GET /pages/domains</c>).
///     <para>
///         Deliberately a different type from <see cref="GitLabPagesDomain" />, because GitLab returns a
///         different shape here: this view names the owning project - the point of an instance-wide sweep -
///         and reduces the certificate to whether it has expired, rather than shipping every certificate on
///         the instance in one response.
///     </para>
/// </summary>
public sealed record GitLabPagesDomainSummary
{
    /// <summary>The custom hostname, for example <c>ssl.domain.example</c>.</summary>
    public required string Domain { get; init; }

    /// <summary>The URL the site is reachable at.</summary>
    public Uri? Url { get; init; }

    /// <summary>The project the domain belongs to.</summary>
    public long? ProjectId { get; init; }

    /// <summary>Whether GitLab has found the verification TXT record for the domain.</summary>
    public bool? Verified { get; init; }

    /// <summary>The value to publish in the domain's <c>_gitlab-pages-verification-code</c> TXT record.</summary>
    public string? VerificationCode { get; init; }

    /// <summary>How long the domain stays enabled without re-verification.</summary>
    public DateTimeOffset? EnabledUntil { get; init; }

    /// <summary>Whether GitLab obtains and renews a Let's Encrypt certificate for the domain automatically.</summary>
    public bool? AutoSslEnabled { get; init; }

    /// <summary>When the domain's certificate expires, and whether it already has.</summary>
    public GitLabPagesDomainCertificateExpiration? CertificateExpiration { get; init; }
}