namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/remote_mirrors/:mirror_id</c>. Every member is optional: unset
///     ones are omitted from the payload rather than sent as null, so a partial update cannot clear a field
///     it never mentioned.
///     <para>
///         There is deliberately no <c>Url</c> here - the PUT schema does not accept one, so a mirror's URL is
///         immutable after creation even though the response carries it.
///     </para>
/// </summary>
public sealed record UpdateRemoteMirrorRequest
{
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