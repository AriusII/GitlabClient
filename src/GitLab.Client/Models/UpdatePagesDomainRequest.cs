namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>PUT /projects/:id/pages/domains/:domain</c> - rotates the certificate on an existing
///     Pages domain, or switches it to Let's Encrypt. The domain itself cannot be renamed; delete and
///     recreate it instead.
///     <para>
///         <see cref="Key" /> is a private key. Treat this record as a secret: never log an instance of it
///         and never put one into an error message.
///     </para>
/// </summary>
public sealed record UpdatePagesDomainRequest
{
    /// <summary>The replacement PEM-encoded certificate chain.</summary>
    public string? Certificate { get; init; }

    /// <summary>
    ///     The PEM-encoded private key matching <see cref="Certificate" />. A secret - see the note on the
    ///     record itself.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    ///     Turns Let's Encrypt issuance on or off. Turning it on discards a user-provided certificate.
    /// </summary>
    public bool? AutoSslEnabled { get; init; }
}