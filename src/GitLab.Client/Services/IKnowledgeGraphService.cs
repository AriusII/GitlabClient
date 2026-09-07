using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Knowledge graph, sitting between the public
///     <c>IKnowledgeGraphClient</c> controller and <c>IKnowledgeGraphRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IKnowledgeGraphService
{
    Task<IReadOnlyList<GitLabKnowledgeGraphNamespace>> ListNamespacesAsync(
        CancellationToken cancellationToken = default);

    Task<GitLabKnowledgeGraphNamespace> EnableNamespaceAsync(string id,
        CancellationToken cancellationToken = default);

    Task DisableNamespaceAsync(string id, CancellationToken cancellationToken = default);
}