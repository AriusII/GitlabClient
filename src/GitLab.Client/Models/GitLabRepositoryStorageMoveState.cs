using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Where a repository storage move has got to. GitLab drives every move - project, group wiki or
///     snippet - through one state machine, so the same six values appear on all three move entities.
/// </summary>
/// <remarks>
///     The vendored spec types <c>state</c> as a bare string with <c>example: scheduled</c> and no
///     <c>enum</c>, but the underlying set is closed: it is GitLab's own <c>RepositoryStorageMove</c>
///     state machine, and these are its six states. Modelling it as an enum is what lets a caller
///     <c>switch</c> on progress instead of comparing magic strings.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabRepositoryStorageMoveState>))]
public enum GitLabRepositoryStorageMoveState
{
    /// <summary>Queued. Nothing has been copied yet.</summary>
    [JsonStringEnumMemberName("scheduled")]
    Scheduled,

    /// <summary>The repository is being copied to the destination shard.</summary>
    [JsonStringEnumMemberName("started")] Started,

    /// <summary>The copy succeeded and the source repository has been cleaned up. The terminal success state.</summary>
    [JsonStringEnumMemberName("finished")] Finished,

    /// <summary>The move failed; <c>error_message</c> carries the reason and the repository stayed on the source shard.</summary>
    [JsonStringEnumMemberName("failed")] Failed,

    /// <summary>The copy succeeded but the source repository has not been removed yet.</summary>
    [JsonStringEnumMemberName("replicated")]
    Replicated,

    /// <summary>
    ///     The copy succeeded and the destination is authoritative, but removing the source repository
    ///     failed. The move is done from a serving point of view; the leftover needs an administrator.
    /// </summary>
    [JsonStringEnumMemberName("cleanup_failed")]
    CleanupFailed
}