namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body for scheduling a repository storage move for one project, group or snippet
///     (<c>POST /projects/:id/repository_storage_moves</c> and its group and snippet siblings).
/// </summary>
public sealed record CreateStorageMoveRequest
{
    /// <summary>
    ///     Storage shard to move the repository to. Leave <c>null</c> to let GitLab pick one from the
    ///     weighted list of storages configured for the instance, which is the usual way to rebalance.
    /// </summary>
    public string? DestinationStorageName { get; init; }
}