namespace GitLab.Client.Models;

/// <summary>The <c>APIEntitiesUserSafe</c> projection embedded by a board-list response.</summary>
public sealed record GitLabSafeUser
{
    public long? Id { get; init; }

    public string? Username { get; init; }

    public string? PublicEmail { get; init; }

    public string? Name { get; init; }
}