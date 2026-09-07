namespace GitLab.Client.Models;

/// <summary>
///     One project repository storage move - the wire's <c>APIEntitiesProjectsRepositoryStorageMove</c>.
///     A project move carries the project's repository together with its wiki and design repositories.
/// </summary>
public sealed record GitLabProjectRepositoryStorageMove
{
    /// <summary>Id of the move itself, not of the project. This is the id the retrieve routes take.</summary>
    public required long Id { get; init; }

    /// <summary>How far the move has got.</summary>
    public required GitLabRepositoryStorageMoveState State { get; init; }

    /// <summary>When the move was scheduled.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Storage shard the repository is moving away from.</summary>
    public string? SourceStorageName { get; init; }

    /// <summary>Storage shard the repository is moving to.</summary>
    public string? DestinationStorageName { get; init; }

    /// <summary>
    ///     Why the move failed. Populated only for
    ///     <see cref="GitLabRepositoryStorageMoveState.Failed" /> and
    ///     <see cref="GitLabRepositoryStorageMoveState.CleanupFailed" />.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    ///     The project being moved, as GitLab's identity-only projection - no <c>visibility</c> and no
    ///     <c>web_url</c>, which is why this is <see cref="GitLabProjectIdentity" /> rather than
    ///     <see cref="GitLabProject" />.
    /// </summary>
    public GitLabProjectIdentity? Project { get; init; }
}