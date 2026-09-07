namespace GitLab.Client.Models;

/// <summary>
///     A project or group access token together with its plaintext secret, as returned by the create
///     and rotate operations (<c>POST /projects/:id/access_tokens</c>,
///     <c>POST /groups/:id/access_tokens</c> and the <c>/rotate</c> forms) - the only three moments
///     GitLab ever discloses it.
///     <para>
///         <see cref="Token" /> cannot be retrieved afterwards: a value read here must be persisted
///         immediately or it is lost, and the read-side operations return the secret-free
///         <see cref="GitLabAccessToken" />. Treat an instance of this type as a credential - do not
///         log it, do not put it in an exception message, and do not let it reach a
///         <c>ToString()</c> on a record (which prints every member).
///     </para>
/// </summary>
public sealed record GitLabAccessTokenWithSecret
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    /// <summary>
    ///     The plaintext token - the credential itself. GitLab returns it exactly once and can never
    ///     show it again.
    /// </summary>
    public string? Token { get; init; }

    public bool? Revoked { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? Description { get; init; }

    /// <summary>The permissions granted to the token. See <see cref="GitLabTokenScopes" />.</summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>The ID of the bot user GitLab created to back this token.</summary>
    public long? UserId { get; init; }

    public DateTimeOffset? LastUsedAt { get; init; }

    public bool? Active { get; init; }

    /// <summary>Whether the token is scoped by <see cref="GranularScopes" /> rather than by <see cref="Scopes" />.</summary>
    public bool? Granular { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>The five most recent unique IP addresses that have authenticated with this token.</summary>
    public IReadOnlyList<string>? LastUsedIps { get; init; }

    public IReadOnlyList<GitLabGranularScope>? GranularScopes { get; init; }

    /// <summary>
    ///     The role the token acts with in the project or group: 10 (Guest), 20 (Reporter),
    ///     30 (Developer), 40 (Maintainer) or 50 (Owner).
    /// </summary>
    public int? AccessLevel { get; init; }

    /// <summary><c>project</c> or <c>group</c>.</summary>
    public string? ResourceType { get; init; }

    public long? ResourceId { get; init; }

    /// <summary>
    ///     Renders the token without its secret. Record types print every member from the
    ///     compiler-generated <c>ToString()</c>, which is exactly how a credential ends up in a log line;
    ///     this override keeps <see cref="Token" /> out of it.
    /// </summary>
    public override string ToString()
    {
        return $"GitLabAccessTokenWithSecret {{ Id = {Id}, Name = {Name}, Token = <redacted> }}";
    }
}