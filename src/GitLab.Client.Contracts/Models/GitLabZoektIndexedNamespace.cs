namespace GitLab.Client.Models;

/// <summary>
///     One namespace indexed on a Zoekt shard (<c>/admin/zoekt/shards/:node_id/indexed_namespaces</c>,
///     <c>/admin/zoekt/namespaces/:id</c>) - GitLab's Zoekt-backed instance code search.
/// </summary>
/// <remarks>Instance-administrator only.</remarks>
public sealed record GitLabZoektIndexedNamespace
{
    public long? Id { get; init; }

    public long? ZoektShardId { get; init; }

    public long? ZoektNodeId { get; init; }

    public long? NamespaceId { get; init; }

    /// <summary>How many replicas this namespace overrides the shard default with, if any.</summary>
    public int? NumberOfReplicasOverride { get; init; }
}