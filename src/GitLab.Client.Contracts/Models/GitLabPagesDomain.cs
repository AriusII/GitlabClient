namespace GitLab.Client.Models;

/// <summary>
///     A Pages custom domain within a project (<c>/projects/:id/pages/domains</c>) - the hostname a
///     project's Pages site is served from, and the TLS certificate serving it.
/// </summary>
public sealed record GitLabPagesDomain
{
    /// <summary>The custom hostname, for example <c>ssl.domain.example</c>.</summary>
    public required string Domain { get; init; }

    /// <summary>The URL the site is reachable at - <c>https://</c> once a certificate is in place.</summary>
    public Uri? Url { get; init; }

    /// <summary>Whether GitLab has found the verification TXT record for the domain.</summary>
    public bool? Verified { get; init; }

    /// <summary>
    ///     The value to publish in the domain's <c>_gitlab-pages-verification-code</c> TXT record. Also
    ///     returned as <c>verification_code</c>.
    /// </summary>
    public string? VerificationCode { get; init; }

    /// <summary>
    ///     How long the domain stays enabled without re-verification. GitLab disables an unverified domain
    ///     once this passes.
    /// </summary>
    public DateTimeOffset? EnabledUntil { get; init; }

    /// <summary>Whether GitLab obtains and renews a Let's Encrypt certificate for the domain automatically.</summary>
    public bool? AutoSslEnabled { get; init; }

    /// <summary>
    ///     The certificate serving the domain, or <see langword="null" /> when the domain is plain HTTP.
    ///     Carries the public certificate only - never the private key.
    /// </summary>
    public GitLabPagesDomainCertificate? Certificate { get; init; }
}