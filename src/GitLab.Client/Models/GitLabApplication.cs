namespace GitLab.Client.Models;

/// <summary>
///     A registered OAuth application, as returned by the read side of the GitLab Applications API -
///     the instance-wide admin surface (<c>GET /applications</c>, <c>DELETE /applications/:id</c>) and
///     the per-user surface (<c>GET /user/applications</c>, <c>GET /user/applications/:id</c>,
///     <c>PUT /user/applications/:id</c>).
///     <para>
///         This type deliberately has no <c>secret</c> member. GitLab discloses the OAuth client
///         secret exactly once per credential - when the application is created, and again when it is
///         renewed - so those two operations return <see cref="GitLabApplicationWithSecret" /> instead.
///         A value of this type provably cannot leak a secret.
///     </para>
/// </summary>
public sealed record GitLabApplication
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

    /// <summary>
    ///     The OAuth scopes this application is limited to. Left as a string list rather than a closed
    ///     enum because GitLab adds scopes without a major version bump; see
    ///     <see cref="GitLabOAuthApplicationScopes" /> for the well-known values.
    /// </summary>
    public IReadOnlyList<string>? Scopes { get; init; }
}