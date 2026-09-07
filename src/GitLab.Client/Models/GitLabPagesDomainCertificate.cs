namespace GitLab.Client.Models;

/// <summary>
///     The TLS certificate serving a Pages custom domain, as returned by the project-scoped Pages domain
///     endpoints.
///     <para>
///         Only the public half is ever returned. GitLab never echoes the private key back on any
///         response, which is why there is no <c>Key</c> member here - the key exists solely on
///         <see cref="CreatePagesDomainRequest" /> and <see cref="UpdatePagesDomainRequest" />.
///     </para>
/// </summary>
public sealed record GitLabPagesDomainCertificate
{
    /// <summary>The certificate's subject distinguished name, for example <c>/CN=ssl.domain.example</c>.</summary>
    public string? Subject { get; init; }

    /// <summary>Whether the certificate is past its expiry date. GitLab keeps serving an expired one.</summary>
    public bool? Expired { get; init; }

    /// <summary>The PEM-encoded public certificate chain.</summary>
    public string? Certificate { get; init; }

    /// <summary>The certificate rendered as human-readable OpenSSL text.</summary>
    public string? CertificateText { get; init; }
}