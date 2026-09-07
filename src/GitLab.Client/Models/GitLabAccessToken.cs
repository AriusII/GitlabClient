namespace GitLab.Client.Models;

/// <summary>
///     A project or group access token as returned by the read side of the GitLab Access Tokens API
///     (<c>GET /projects/:id/access_tokens</c>, <c>GET /groups/:id/access_tokens</c> and their
///     <c>/:token_id</c> forms).
///     <para>
///         This type deliberately has no <c>token</c> member. GitLab hands back the plaintext secret
///         exactly once - on create and on rotate - and never again, so those operations return
///         <see cref="GitLabAccessTokenWithSecret" /> instead. Keeping them as two types means the
///         signature of a call tells you whether it can hand you a secret, and a value of this type
///         provably cannot leak one.
///     </para>
/// </summary>
public sealed record GitLabAccessToken
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public bool? Revoked { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? Description { get; init; }

    /// <summary>
    ///     The permissions granted to the token - for example <c>api</c>, <c>read_repository</c>. The
    ///     well-known values are listed on <see cref="GitLabTokenScopes" />; the wire type is left
    ///     <see cref="string" /> because GitLab adds scopes without a major version bump and the spec
    ///     does not enumerate them, so a closed enum here would turn a new scope into a hard
    ///     deserialization failure on an otherwise fine listing.
    /// </summary>
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
    ///     The role the token acts with in the project or group. GitLab returns 10 (Guest), 20 (Reporter),
    ///     30 (Developer), 40 (Maintainer) or 50 (Owner); left an <see cref="int" /> because the accepted
    ///     set on create is wider than the set documented on the response.
    /// </summary>
    public int? AccessLevel { get; init; }

    /// <summary><c>project</c> or <c>group</c>.</summary>
    public string? ResourceType { get; init; }

    public long? ResourceId { get; init; }
}