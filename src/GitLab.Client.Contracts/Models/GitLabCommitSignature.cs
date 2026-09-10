namespace GitLab.Client.Models;

/// <summary>
///     The signature on a signed commit (<c>GET /projects/:id/repository/commits/:sha/signature</c>).
///     GitLab answers <c>404</c> for an unsigned commit, which surfaces as a
///     <see cref="Abstractions.Exceptions.GitLabNotFoundException" />.
/// </summary>
/// <remarks>
///     The payload is a flat union keyed on <see cref="SignatureType" />: a <c>PGP</c> signature fills the
///     <c>Gpg*</c> members, an <c>X509</c> signature fills <see cref="X509Certificate" />, and an
///     <c>SSH</c> signature fills <see cref="Key" />. Everything outside the matching arm stays
///     <see langword="null" />, so the type is nullable throughout rather than split into three records
///     the caller would have to switch over before it could read <see cref="VerificationStatus" />.
/// </remarks>
public sealed record GitLabCommitSignature
{
    /// <summary>GitLab's own wire value - <c>PGP</c>, <c>X509</c> or <c>SSH</c>.</summary>
    public string? SignatureType { get; init; }

    /// <summary>
    ///     Whether GitLab could verify the signature: <c>verified</c>, <c>unverified</c>,
    ///     <c>unverified_key</c>, <c>unknown_key</c>, <c>same_user_different_email</c> and friends. Left a
    ///     string because GitLab keeps extending the vocabulary.
    /// </summary>
    public string? VerificationStatus { get; init; }

    /// <summary>Where GitLab read the signature from, normally <c>gitaly</c>.</summary>
    public string? CommitSource { get; init; }

    /// <summary>The id of the GPG key GitLab matched, for a <c>PGP</c> signature.</summary>
    public long? GpgKeyId { get; init; }

    /// <summary>The primary key id of the matched GPG key.</summary>
    public string? GpgKeyPrimaryKeyid { get; init; }

    /// <summary>The name on the matched GPG key.</summary>
    public string? GpgKeyUserName { get; init; }

    /// <summary>The email address on the matched GPG key.</summary>
    public string? GpgKeyUserEmail { get; init; }

    /// <summary>The subkey id, when the commit was signed with a subkey rather than the primary key.</summary>
    public string? GpgKeySubkeyId { get; init; }

    /// <summary>The certificate GitLab matched, for an <c>X509</c> signature.</summary>
    public GitLabX509Certificate? X509Certificate { get; init; }

    /// <summary>The SSH key GitLab matched, for an <c>SSH</c> signature.</summary>
    public GitLabSshKey? Key { get; init; }
}