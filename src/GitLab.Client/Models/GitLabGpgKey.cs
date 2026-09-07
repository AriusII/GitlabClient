namespace GitLab.Client.Models;

/// <summary>
///     A GPG key belonging to a user, as returned by the GitLab Keys API (<c>/user/gpg_keys</c> and
///     <c>/users/:id/gpg_keys</c>). GitLab uses these to verify commit signatures.
/// </summary>
public sealed record GitLabGpgKey
{
    public required long Id { get; init; }

    /// <summary>
    ///     The ASCII-armored public key, beginning <c>-----BEGIN PGP PUBLIC KEY BLOCK-----</c>. Kept a
    ///     <see cref="string" /> verbatim: it is multi-line armored text, not something to parse here.
    /// </summary>
    public required string Key { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}