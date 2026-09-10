namespace GitLab.Client.Models;

/// <summary>
///     The reduced group projection assigned to a runner - the wire's
///     <c>APIEntitiesBasicGroupDetails</c> projection.
/// </summary>
public sealed record GitLabRunnerGroup
{
    public required long Id { get; init; }

    public string? Name { get; init; }

    public Uri? WebUrl { get; init; }
}