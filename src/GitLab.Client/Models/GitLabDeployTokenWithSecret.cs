namespace GitLab.Client.Models;

/// <summary>
///     A newly created deploy token together with its plaintext password, as returned by
///     <c>POST /projects/:id/deploy_tokens</c> and <c>POST /groups/:id/deploy_tokens</c> - the only
///     moment GitLab ever discloses it.
///     <para>
///         <see cref="Token" /> cannot be retrieved afterwards: a value read here must be persisted
///         immediately or the token is useless and has to be recreated. Every read-side operation
///         returns the secret-free <see cref="GitLabDeployToken" /> instead.
///     </para>
///     <para>
///         Treat an instance of this type as a credential - do not log it, do not put it in an
///         exception message, and do not hand it to a generic object dumper. <see cref="ToString" />
///         is overridden here for exactly that reason.
///     </para>
/// </summary>
public sealed record GitLabDeployTokenWithSecret
{
    public required long Id { get; init; }

    /// <summary>The human-readable name given to the token when it was created.</summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The username half of the credential - <c>gitlab+deploy-token-{n}</c> unless one was supplied
    ///     on create. Safe to log: on its own it authenticates nothing.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    ///     The plaintext token - the password half of the credential, and on its own enough to pull from
    ///     the registry and clone the repository. GitLab returns it exactly once and can never show it
    ///     again.
    /// </summary>
    public string? Token { get; init; }

    /// <summary>When the token stops working. <see langword="null" /> means it never expires.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>What the token may do. See <see cref="GitLabDeployTokenScopes" />.</summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>Whether the token has been revoked.</summary>
    public bool? Revoked { get; init; }

    /// <summary>Whether <see cref="ExpiresAt" /> is in the past.</summary>
    public bool? Expired { get; init; }

    /// <summary>
    ///     Renders the token without its secret. Record types print every member from the
    ///     compiler-generated <c>ToString()</c>, which is exactly how a credential ends up in a log line;
    ///     this override keeps <see cref="Token" /> out of it.
    /// </summary>
    public override string ToString()
    {
        return $"GitLabDeployTokenWithSecret {{ Id = {Id}, Name = {Name}, Username = {Username}, Token = <redacted> }}";
    }
}