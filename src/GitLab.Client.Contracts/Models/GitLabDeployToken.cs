namespace GitLab.Client.Models;

/// <summary>
///     A deploy token as returned by the read side of the GitLab Deploy Tokens API
///     (<c>GET /deploy_tokens</c>, <c>GET /projects/:id/deploy_tokens</c>,
///     <c>GET /groups/:id/deploy_tokens</c> and their <c>/:token_id</c> forms).
///     <para>
///         A deploy token is a username/password pair that can clone a repository and pull from the
///         container and package registries without belonging to a user. It is not a deploy
///         <em>key</em>: those are SSH public keys and live on <see cref="GitLabDeployKey" />.
///     </para>
///     <para>
///         This type deliberately has no <c>token</c> member. GitLab discloses the password exactly
///         once - in the response to the create call - and never again, so the create operations return
///         <see cref="GitLabDeployTokenWithSecret" /> instead. Keeping them as two types means the
///         signature of a call tells you whether it can hand you a secret, and a value of this type
///         provably cannot leak one.
///     </para>
/// </summary>
public sealed record GitLabDeployToken
{
    public required long Id { get; init; }

    /// <summary>The human-readable name given to the token when it was created.</summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The username half of the credential - <c>gitlab+deploy-token-{n}</c> unless one was supplied
    ///     on create. Safe to log: on its own it authenticates nothing.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>When the token stops working. <see langword="null" /> means it never expires.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    ///     What the token may do - for example <c>read_repository</c>, <c>write_registry</c>. The
    ///     well-known values are listed on <see cref="GitLabDeployTokenScopes" />; the wire type is left
    ///     <see cref="string" /> because GitLab types this array without an item schema on the response
    ///     and adds scopes in minor releases, so a closed enum here would turn a new scope into a hard
    ///     deserialization failure on an otherwise fine listing.
    /// </summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>Whether the token has been revoked. A revoked token is kept but no longer authenticates.</summary>
    public bool? Revoked { get; init; }

    /// <summary>Whether <see cref="ExpiresAt" /> is in the past.</summary>
    public bool? Expired { get; init; }
}