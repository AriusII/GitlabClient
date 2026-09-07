namespace GitLab.Client.Models;

/// <summary>
///     The trimmed group shape (<c>APIEntitiesPublicGroupDetails</c>) GitLab uses where it lists
///     namespaces rather than describing them: <c>GET /projects/:id/groups</c> (the project's ancestor
///     groups) and <c>GET /projects/:id/transfer_locations</c>.
/// </summary>
public sealed record GitLabPublicGroupDetails
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required Uri WebUrl { get; init; }

    /// <summary>The group's name qualified by its ancestors, for example "GitLab.org / Charts".</summary>
    public string? FullName { get; init; }

    /// <summary>The group's path qualified by its ancestors, for example "gitlab-org/charts".</summary>
    public string? FullPath { get; init; }

    public Uri? AvatarUrl { get; init; }
}