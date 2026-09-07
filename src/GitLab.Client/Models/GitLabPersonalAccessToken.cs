namespace GitLab.Client.Models;

/// <summary>
///     A personal access token as returned by the read side of the GitLab Access Tokens API
///     (<c>GET /personal_access_tokens</c>, <c>GET /personal_access_tokens/:id</c>,
///     <c>GET /personal_access_tokens/self</c> and the service-account listings).
///     <para>
///         This type deliberately has no <c>token</c> member. GitLab hands back the plaintext secret
///         exactly once - on create and on rotate - and never again, so those operations return
///         <see cref="GitLabPersonalAccessTokenWithSecret" /> instead.
///     </para>
/// </summary>
public sealed record GitLabPersonalAccessToken
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public bool? Revoked { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? Description { get; init; }

    /// <summary>
    ///     The permissions granted to the token - for example <c>api</c>, <c>read_user</c>. The
    ///     well-known values are listed on <see cref="GitLabTokenScopes" />.
    /// </summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>The ID of the user the token authenticates as.</summary>
    public long? UserId { get; init; }

    public DateTimeOffset? LastUsedAt { get; init; }

    public bool? Active { get; init; }

    /// <summary>Whether the token is scoped by <see cref="GranularScopes" /> rather than by <see cref="Scopes" />.</summary>
    public bool? Granular { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    ///     The five most recent unique IP addresses that have authenticated with this token. Absent from
    ///     the service-account and association listings, which project a narrower entity.
    /// </summary>
    public IReadOnlyList<string>? LastUsedIps { get; init; }

    public IReadOnlyList<GitLabGranularScope>? GranularScopes { get; init; }
}