namespace GitLab.Client.Models;

/// <summary>
///     One project reachable by the authenticating token, nested inside
///     <see cref="GitLabTokenAssociations" />. A narrower projection than <see cref="GitLabProject" />:
///     the association listing returns only enough to identify the project and the caller's role in it.
/// </summary>
public sealed record GitLabTokenAssociationProject
{
    public required long Id { get; init; }

    public string? Name { get; init; }

    public string? NameWithNamespace { get; init; }

    public string? Path { get; init; }

    public string? PathWithNamespace { get; init; }

    public string? Description { get; init; }

    public Uri? WebUrl { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>The token owner's role in the project: 10 (Guest) through 50 (Owner).</summary>
    public int? AccessLevel { get; init; }

    /// <summary><c>private</c>, <c>internal</c> or <c>public</c>.</summary>
    public string? Visibility { get; init; }
}