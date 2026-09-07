namespace GitLab.Client.Models;

/// <summary>
///     The X.509 signature on a signed tag
///     (<c>GET /projects/:id/repository/tags/:tag_name/signature</c>). GitLab answers <c>404</c> for an
///     unsigned tag, which surfaces as a <see cref="Abstractions.Exceptions.GitLabNotFoundException" />.
/// </summary>
public sealed record GitLabTagSignature
{
    /// <summary>GitLab's own wire value; <c>X509</c> is the only kind of tag signature it exposes here.</summary>
    public string? SignatureType { get; init; }

    /// <summary>Whether GitLab could verify the signature, for example <c>verified</c> or <c>unverified</c>.</summary>
    public string? VerificationStatus { get; init; }

    /// <summary>The certificate the tag was signed with.</summary>
    public GitLabX509Certificate? X509Certificate { get; init; }
}