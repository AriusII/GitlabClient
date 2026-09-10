namespace GitLab.Client.Models;

/// <summary>
///     One snippet repository storage move - the wire's <c>APIEntitiesSnippetsRepositoryStorageMove</c>.
/// </summary>
public sealed record GitLabSnippetRepositoryStorageMove
{
    /// <summary>Id of the move itself, not of the snippet. This is the id the retrieve routes take.</summary>
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

    /// <summary>The snippet being moved.</summary>
    public GitLabStorageMoveSnippet? Snippet { get; init; }
}