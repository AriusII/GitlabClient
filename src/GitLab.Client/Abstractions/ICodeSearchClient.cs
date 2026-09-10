using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's Zoekt-backed instance code search admin API (<c>/admin/zoekt/...</c>) - the search
///     nodes ("shards") that index and serve code search, and which namespaces are indexed on each.
/// </summary>
/// <remarks>
///     Every endpoint here requires instance-administrator access; GitLab answers <c>403</c> for anyone
///     else. Zoekt-backed code search is an evolving GitLab surface, so response DTOs are deliberately
///     nullable-permissive.
/// </remarks>
public interface ICodeSearchClient
{
    /// <summary>
    ///     Overrides how many replicas an enabled namespace is indexed with. <paramref name="id" /> is the
    ///     namespace's numeric ID or its URL-encoded full path; pass <c>null</c> for
    ///     <paramref name="request" /> to send an empty update body.
    /// </summary>
    Task<GitLabZoektIndexedNamespace> UpdateNamespaceReplicasAsync(string id,
        UpdateZoektNamespaceReplicasRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Triggers Zoekt indexing for one project, returning the ID of the background job GitLab enqueued.</summary>
    Task<GitLabZoektProjectIndexResult> IndexProjectAsync(long projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets every Zoekt search node configured on this instance.</summary>
    Task<IReadOnlyList<GitLabZoektNode>> ListShardsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets every namespace indexed on one Zoekt node.</summary>
    Task<IReadOnlyList<GitLabZoektIndexedNamespace>> ListIndexedNamespacesAsync(long nodeId,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a namespace from a node's Zoekt index.</summary>
    Task RemoveIndexedNamespaceAsync(long nodeId, long namespaceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a namespace to a node for Zoekt indexing. Pass <c>null</c> for <paramref name="request" />
    ///     to send an empty body.
    /// </summary>
    Task<GitLabZoektIndexedNamespace> AddIndexedNamespaceAsync(long nodeId, long namespaceId,
        AddZoektIndexedNamespaceRequest? request = null, CancellationToken cancellationToken = default);
}