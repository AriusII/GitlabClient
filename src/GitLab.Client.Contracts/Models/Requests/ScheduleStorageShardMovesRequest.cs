namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body for draining a whole storage shard: schedules a repository storage move for every project,
///     group or snippet currently stored on <see cref="SourceStorageName" />
///     (<c>POST /project_repository_storage_moves</c> and its group and snippet siblings).
/// </summary>
public sealed record ScheduleStorageShardMovesRequest
{
    /// <summary>Storage shard to drain. Required - this is what selects the repositories to move.</summary>
    public required string SourceStorageName { get; init; }

    /// <summary>
    ///     Storage shard to move everything to. Leave <c>null</c> to let GitLab pick a destination per
    ///     repository from the weighted list of storages configured for the instance.
    /// </summary>
    public string? DestinationStorageName { get; init; }
}