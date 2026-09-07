namespace GitLab.Client.Models;

/// <summary>
///     An impersonation token together with its plaintext secret, as returned by
///     <c>POST /users/:user_id/impersonation_tokens</c> - the only moment GitLab discloses it.
///     <para>
///         An impersonation token authenticates as another user and can perform both API calls and Git
///         reads and writes, so it is the most dangerous credential this library can hand back. Persist
///         <see cref="Token" /> immediately or lose it; never log it.
///     </para>
/// </summary>
public sealed record GitLabImpersonationTokenWithSecret
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    /// <summary>
    ///     The plaintext token - the credential itself. GitLab returns it exactly once and can never
    ///     show it again.
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    ///     Always <see langword="true" /> on this route. See
    ///     <see cref="GitLabImpersonationToken.Impersonation" /> for why this is a boolean and not the
    ///     string the pinned spec declares.
    /// </summary>
    public bool? Impersonation { get; init; }

    public bool? Revoked { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? Description { get; init; }

    /// <summary>The permissions granted to the token. See <see cref="GitLabTokenScopes" />.</summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>The ID of the user the token impersonates.</summary>
    public long? UserId { get; init; }

    public DateTimeOffset? LastUsedAt { get; init; }

    public bool? Active { get; init; }

    /// <summary>Whether the token is scoped by <see cref="GranularScopes" /> rather than by <see cref="Scopes" />.</summary>
    public bool? Granular { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>The five most recent unique IP addresses that have authenticated with this token.</summary>
    public IReadOnlyList<string>? LastUsedIps { get; init; }

    public IReadOnlyList<GitLabGranularScope>? GranularScopes { get; init; }

    /// <summary>Renders the token without its secret, so a stray log line cannot print the credential.</summary>
    public override string ToString()
    {
        return $"GitLabImpersonationTokenWithSecret {{ Id = {Id}, Name = {Name}, Token = <redacted> }}";
    }
}