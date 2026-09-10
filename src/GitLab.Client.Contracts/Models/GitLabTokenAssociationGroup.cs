namespace GitLab.Client.Models;

/// <summary>
///     One group reachable by the authenticating token, nested inside
///     <see cref="GitLabTokenAssociations" />. A narrower projection than <see cref="GitLabGroup" />:
///     the association listing returns only enough to identify the group and the caller's role in it.
/// </summary>
public sealed record GitLabTokenAssociationGroup
{
    public required long Id { get; init; }

    public string? Name { get; init; }

    public Uri? WebUrl { get; init; }

    /// <summary><see langword="null" /> for a top-level group.</summary>
    public long? ParentId { get; init; }

    public long? OrganizationId { get; init; }

    /// <summary>The token owner's role in the group: 10 (Guest) through 50 (Owner).</summary>
    public int? AccessLevel { get; init; }

    /// <summary><c>private</c>, <c>internal</c> or <c>public</c>.</summary>
    public string? Visibility { get; init; }
}