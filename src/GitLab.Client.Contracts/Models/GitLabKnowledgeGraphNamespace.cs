namespace GitLab.Client.Models;

/// <summary>
///     A namespace enabled for GitLab's Knowledge Graph (<c>/admin/knowledge_graph/namespaces</c>) - the
///     graph of code entities and their relationships that powers Duo's code-aware features.
/// </summary>
/// <remarks>Instance-administrator only.</remarks>
public sealed record GitLabKnowledgeGraphNamespace
{
    public long? Id { get; init; }

    public long? RootNamespaceId { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}