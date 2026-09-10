namespace GitLab.Client.Models;

/// <summary>
///     One SSH key belonging to an enterprise user of a group, as returned by the group credentials
///     inventory (<c>GET /groups/:id/manage/ssh_keys</c>).
///     <para>
///         Deliberately not <see cref="GitLabSshKey" />: this listing never includes the public key
///         material itself, only its metadata, so reusing the <c>/user/keys</c> shape - whose
///         <see cref="GitLabSshKey.Key" /> member is required - would risk a deserialization failure the
///         moment GitLab omits a field this endpoint never promised.
///     </para>
/// </summary>
public sealed record GitLabGroupManagedSshKey
{
    public required long Id { get; init; }

    /// <summary>The human-readable label the key's owner gave it.</summary>
    public string? Title { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public DateTimeOffset? LastUsedAt { get; init; }

    /// <summary>What the key may be used for: <c>auth_and_signing</c>, <c>auth</c> or <c>signing</c>.</summary>
    public string? UsageType { get; init; }

    /// <summary>The ID of the enterprise user who owns the key.</summary>
    public long? UserId { get; init; }
}