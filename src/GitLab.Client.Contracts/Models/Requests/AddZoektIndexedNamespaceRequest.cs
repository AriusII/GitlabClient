namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body for <c>PUT /admin/zoekt/shards/:node_id/indexed_namespaces/:namespace_id</c>, which adds a
///     namespace to a Zoekt node for indexing.
/// </summary>
public sealed record AddZoektIndexedNamespaceRequest
{
    /// <summary>Whether the indexed namespace should be enabled for searching, as opposed to indexing only.</summary>
    public bool? Search { get; init; }
}