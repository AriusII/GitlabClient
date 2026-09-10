namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /groups/:id/ssh_certificates</c>.</summary>
public sealed record CreateSshCertificateRequest
{
    /// <summary>The human-readable label for the certificate authority.</summary>
    public required string Title { get; init; }

    /// <summary>
    ///     The certificate authority's public key, in <c>authorized_keys</c> form. A long single-line
    ///     string containing spaces, which is why it travels in the JSON body rather than the URL.
    /// </summary>
    public required string Key { get; init; }
}