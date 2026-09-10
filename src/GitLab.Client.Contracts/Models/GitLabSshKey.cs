namespace GitLab.Client.Models;

/// <summary>
///     An SSH key belonging to a user, as returned by the GitLab Keys API (<c>/user/keys</c> and
///     <c>/users/:id/keys</c>).
/// </summary>
public sealed record GitLabSshKey
{
    public required long Id { get; init; }

    /// <summary>The human-readable label for the key.</summary>
    public required string Title { get; init; }

    /// <summary>The public key itself, in <c>authorized_keys</c> form.</summary>
    public required string Key { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public DateTimeOffset? LastUsedAt { get; init; }

    /// <summary>What the key may be used for: <c>auth_and_signing</c>, <c>auth</c> or <c>signing</c>.</summary>
    public string? UsageType { get; init; }

    /// <summary>
    ///     The key's owner. Only the instance-wide administrator lookup <c>GET /keys/:id</c> embeds this;
    ///     every other keys endpoint already scopes the request to a user and leaves it <see langword="null" />.
    /// </summary>
    public GitLabUser? User { get; init; }
}