using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One ActiveContext collection - a named group of indexed queues (for example
///     <c>gitlab_active_context_code</c>) with its own queue-sharding options
///     (<c>/admin/active_context/collections/:id</c>).
/// </summary>
/// <remarks>Instance-administrator only.</remarks>
public sealed record GitLabActiveContextCollectionDetail
{
    public long? Id { get; init; }

    public string? Name { get; init; }

    public long? ConnectionId { get; init; }

    /// <summary>
    ///     Queue-sharding options such as <c>queue_shard_count</c> and <c>queue_shard_limit</c>. The spec
    ///     types this as an untyped object, so it is carried as a raw <see cref="JsonElement" /> rather than
    ///     forced into a shape GitLab does not promise.
    /// </summary>
    public JsonElement? Options { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}