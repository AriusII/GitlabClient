using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's Knowledge Graph admin API (<c>/admin/knowledge_graph/namespaces</c>) - the graph of
///     code entities and their relationships that powers Duo's code-aware features, and which namespaces
///     have it enabled.
/// </summary>
/// <remarks>
///     Every endpoint here requires instance-administrator access; GitLab answers <c>403</c> for anyone
///     else. Knowledge Graph is an evolving GitLab surface, so response DTOs are deliberately
///     nullable-permissive.
/// </remarks>
public interface IKnowledgeGraphClient
{
    /// <summary>Gets every namespace enabled for Knowledge Graph.</summary>
    Task<IReadOnlyList<GitLabKnowledgeGraphNamespace>> ListNamespacesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Enables Knowledge Graph for a namespace. <paramref name="id" /> is the namespace's numeric ID or
    ///     its URL-encoded full path.
    /// </summary>
    Task<GitLabKnowledgeGraphNamespace> EnableNamespaceAsync(string id,
        CancellationToken cancellationToken = default);

    /// <summary>Disables Knowledge Graph for a namespace.</summary>
    Task DisableNamespaceAsync(string id, CancellationToken cancellationToken = default);
}