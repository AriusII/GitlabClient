namespace GitLab.Client.Models;

/// <summary>
///     One Zoekt search node (<c>/admin/zoekt/shards</c>) - a running instance of the Zoekt indexing and
///     search service that powers GitLab's instance code search.
/// </summary>
/// <remarks>Instance-administrator only.</remarks>
public sealed record GitLabZoektNode
{
    public long? Id { get; init; }

    /// <summary>Where this node's indexer accepts indexing requests.</summary>
    public Uri? IndexBaseUrl { get; init; }

    /// <summary>Where this node answers search queries.</summary>
    public Uri? SearchBaseUrl { get; init; }
}