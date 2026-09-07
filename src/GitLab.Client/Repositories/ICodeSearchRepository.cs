using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Code search resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ICodeSearchService), typeof(ICodeSearchClient))]
internal interface ICodeSearchRepository
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