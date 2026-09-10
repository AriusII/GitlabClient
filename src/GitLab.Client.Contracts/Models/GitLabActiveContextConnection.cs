namespace GitLab.Client.Models;

/// <summary>
///     One ActiveContext connection - a configured backend (currently Elasticsearch or OpenSearch) that
///     GitLab's semantic search / knowledge-graph indexing pipeline writes to and reads from
///     (<c>/admin/active_context/connections</c>). Exactly one connection is <see cref="Active" /> at a
///     time; activating a new one deactivates whichever was active before.
/// </summary>
/// <remarks>
///     Instance-administrator only. ActiveContext is an evolving GitLab surface, so every member here is
///     nullable even where GitLab is expected to always populate it.
/// </remarks>
public sealed record GitLabActiveContextConnection
{
    public long? Id { get; init; }

    public string? Name { get; init; }

    /// <summary>The fully qualified Ruby class implementing this connection's backend adapter.</summary>
    public string? AdapterClass { get; init; }

    /// <summary>The prefix ActiveContext namespaces every index/collection name under for this connection.</summary>
    public string? Prefix { get; init; }

    /// <summary>Whether this is the connection ActiveContext currently indexes and searches through.</summary>
    public bool? Active { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}