using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Code search, sitting between the public
///     <c>ICodeSearchClient</c> controller and <c>ICodeSearchRepository</c>'s raw GitLab access. Mirrors
///     the repository's method shapes 1:1 today (its implementation is generated); this is the seam
///     where request validation, caching, or cross-resource composition would go once the resource needs
///     more than pass-through.
/// </summary>
internal interface ICodeSearchService
{
    Task<GitLabZoektIndexedNamespace> UpdateNamespaceReplicasAsync(string id,
        UpdateZoektNamespaceReplicasRequest? request = null, CancellationToken cancellationToken = default);

    Task<GitLabZoektProjectIndexResult> IndexProjectAsync(long projectId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GitLabZoektNode>> ListShardsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GitLabZoektIndexedNamespace>> ListIndexedNamespacesAsync(long nodeId,
        CancellationToken cancellationToken = default);

    Task RemoveIndexedNamespaceAsync(long nodeId, long namespaceId, CancellationToken cancellationToken = default);

    Task<GitLabZoektIndexedNamespace> AddIndexedNamespaceAsync(long nodeId, long namespaceId,
        AddZoektIndexedNamespaceRequest? request = null, CancellationToken cancellationToken = default);
}