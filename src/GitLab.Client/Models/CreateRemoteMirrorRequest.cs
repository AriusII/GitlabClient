using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/remote_mirrors</c>.
///     <para>
///         <see cref="OnlyProtectedBranches" /> and <see cref="MirrorBranchRegex" /> are mutually exclusive;
///         setting both is rejected by GitLab rather than validated here, so the server stays the single
///         authority on the rule.
///     </para>
/// </summary>
public sealed record CreateRemoteMirrorRequest
{
    /// <summary>
    ///     The remote URL to push to, credentials included where the auth method needs them. A
    ///     <see cref="string" /> for symmetry with <see cref="GitLabRemoteMirror.Url" />, which GitLab returns
    ///     with its userinfo scrubbed to <c>*****:*****</c>.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "Symmetric with GitLabRemoteMirror.Url, whose scrubbed '*****:*****' userinfo Uri cannot be relied on to parse.")]
    public required string Url { get; init; }

    public bool? Enabled { get; init; }

    /// <summary><c>ssh_public_key</c> or <c>password</c>.</summary>
    public string? AuthMethod { get; init; }

    public bool? KeepDivergentRefs { get; init; }

    /// <summary>Mutually exclusive with <see cref="MirrorBranchRegex" />.</summary>
    public bool? OnlyProtectedBranches { get; init; }

    /// <summary>Mutually exclusive with <see cref="OnlyProtectedBranches" />.</summary>
    public string? MirrorBranchRegex { get; init; }

    /// <summary>
    ///     SSH host keys in bare (<c>ssh-ed25519 AAAA...</c>) or full <c>known_hosts</c> format. Sent as raw
    ///     strings; GitLab reports them back as <see cref="GitLabMirrorHostKey" /> fingerprints.
    /// </summary>
    public IReadOnlyList<string>? HostKeys { get; init; }
}