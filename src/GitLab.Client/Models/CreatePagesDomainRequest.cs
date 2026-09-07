namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /projects/:id/pages/domains</c>.
///     <para>
///         <see cref="Key" /> is a private key. Treat this record as a secret: it is write-only as far as
///         GitLab is concerned - no response ever echoes the key back - so never log an instance of it and
///         never put one into an error message.
///     </para>
/// </summary>
/// <remarks>
///     The spec also lists <c>user_provided_certificate</c> and <c>user_provided_key</c>. Those are Grape's
///     internal <c>as:</c> targets for <see cref="Certificate" /> and <see cref="Key" />, not accepted wire
///     names, so they are deliberately not exposed: sending them would be silently ignored.
/// </remarks>
public sealed record CreatePagesDomainRequest
{
    /// <summary>The custom hostname to serve the project's Pages site from. Required.</summary>
    public required string Domain { get; init; }

    /// <summary>
    ///     The PEM-encoded certificate chain. Required together with <see cref="Key" /> unless
    ///     <see cref="AutoSslEnabled" /> is set, in which case GitLab issues the certificate itself.
    /// </summary>
    public string? Certificate { get; init; }

    /// <summary>
    ///     The PEM-encoded private key matching <see cref="Certificate" />. A secret - see the note on the
    ///     record itself.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    ///     Asks GitLab to obtain and renew a Let's Encrypt certificate for the domain instead of taking one
    ///     from <see cref="Certificate" />.
    /// </summary>
    public bool? AutoSslEnabled { get; init; }
}