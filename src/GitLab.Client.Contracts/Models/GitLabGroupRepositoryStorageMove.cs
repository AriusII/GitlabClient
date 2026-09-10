namespace GitLab.Client.Models;

/// <summary>
///     One group repository storage move - the wire's <c>APIEntitiesGroupsRepositoryStorageMove</c>.
/// </summary>
/// <remarks>
///     A group move relocates the <em>group wiki</em> repository only. It does not touch the
///     repositories of the projects inside the group; those need project moves of their own.
/// </remarks>
public sealed record GitLabGroupRepositoryStorageMove
{
    /// <summary>Id of the move itself, not of the group. This is the id the retrieve routes take.</summary>
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

    /// <summary>The group whose wiki is being moved.</summary>
    public GitLabStorageMoveGroup? Group { get; init; }
}