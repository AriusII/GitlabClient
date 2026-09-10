namespace GitLab.Client.Models;

/// <summary>
///     A registered OAuth application together with its plaintext client secret, as returned by the
///     create and renew-secret operations (<c>POST /applications</c>, <c>POST /user/applications</c>,
///     <c>POST /applications/:id/renew-secret</c>) - the only moments GitLab ever discloses it.
///     <para>
///         <see cref="Secret" /> cannot be retrieved afterwards: a value read here must be persisted
///         immediately or it is lost, and every read-side operation returns the secret-free
///         <see cref="GitLabApplication" /> instead. Treat an instance of this type as a credential -
///         do not log it, do not put it in an exception message, and do not let it reach a
///         <c>ToString()</c> on a record (which prints every member).
///     </para>
/// </summary>
public sealed record GitLabApplicationWithSecret
{
    public required long Id { get; init; }

    /// <summary>The OAuth <c>client_id</c> GitLab generated for this application.</summary>
    public string? ApplicationId { get; init; }

    /// <summary>The display name given to the application.</summary>
    public string? ApplicationName { get; init; }

    /// <summary>The redirect URI GitLab sends the OAuth authorization code or token back to.</summary>
    public Uri? CallbackUrl { get; init; }

    /// <summary>
    ///     Whether the application is confidential - able to keep its client secret private (a
    ///     server-side app), as opposed to public (a native or single-page app that cannot).
    /// </summary>
    public bool? Confidential { get; init; }

    /// <summary>The OAuth scopes this application is limited to. See <see cref="GitLabOAuthApplicationScopes" />.</summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>
    ///     The plaintext OAuth client secret - the credential itself. GitLab returns it exactly once
    ///     per issuance (on create, and again on renew) and can never show it again.
    /// </summary>
    public string? Secret { get; init; }

    /// <summary>
    ///     Renders the application without its secret. Record types print every member from the
    ///     compiler-generated <c>ToString()</c>, which is exactly how a credential ends up in a log
    ///     line; this override keeps <see cref="Secret" /> out of it.
    /// </summary>
    public override string ToString()
    {
        return $"GitLabApplicationWithSecret {{ Id = {Id}, ApplicationName = {ApplicationName}, Secret = <redacted> }}";
    }
}