namespace GitLab.Client.Models;

/// <summary>
///     One SSH host key recorded against a <see cref="GitLabRemoteMirror" />. Asymmetric with the request
///     side on purpose: a mirror is created or updated with an array of raw host-key strings, and GitLab
///     answers with these fingerprint objects.
/// </summary>
public sealed record GitLabMirrorHostKey
{
    /// <summary>The key's SHA-256 fingerprint, for example <c>SHA256:abcd1234</c>.</summary>
    public string? FingerprintSha256 { get; init; }
}