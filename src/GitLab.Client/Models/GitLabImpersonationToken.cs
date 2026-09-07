namespace GitLab.Client.Models;

/// <summary>
///     An impersonation token as returned by the read side of
///     <c>/users/:user_id/impersonation_tokens</c>. Administrators create these to act on behalf of a
///     user; they are invisible to that user on their own profile page.
///     <para>
///         Like every other token entity here, the read side carries no secret - the create operation
///         returns <see cref="GitLabImpersonationTokenWithSecret" /> instead.
///     </para>
/// </summary>
public sealed record GitLabImpersonationToken
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    /// <summary>
    ///     Always <see langword="true" /> on this route - the flag exists because impersonation tokens
    ///     share a table with personal access tokens.
    ///     <para>
    ///         The pinned spec types this member as a string; GitLab returns the underlying boolean
    ///         column, and the documented example payload shows <c>"impersonation": true</c>, so it is
    ///         modelled as a nullable <see cref="bool" />.
    ///     </para>
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
}