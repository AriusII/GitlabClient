using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class KnowledgeGraphClient(IGitLabApiConnection connection) : IKnowledgeGraphClient
{
    public async Task<IReadOnlyList<GitLabKnowledgeGraphNamespace>> ListNamespacesAsync(
        CancellationToken cancellationToken = default)
    {
        // One unpaginated array of enabled namespaces, so there is nothing for GetPagedAsync to
        // follow - the result is materialized rather than streamed.
        return await connection.GetAsync(
                NamespacesRoute().Build(),
                GitLabJsonContext.Default.GitLabKnowledgeGraphNamespaceArray,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<GitLabKnowledgeGraphNamespace> EnableNamespaceAsync(string id,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            NamespacesRoute().Escaped(id).Build(),
            GitLabJsonContext.Default.GitLabKnowledgeGraphNamespace,
            cancellationToken);
    }

    public Task DisableNamespaceAsync(string id, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            NamespacesRoute().Escaped(id).Build(),
            cancellationToken);
    }

    /// <summary>"admin" and "knowledge_graph" come from the route template, never from a caller.</summary>
    private static GitLabRouteBuilder NamespacesRoute()
    {
        return GitLabRouteBuilder.Create("admin").Literal("knowledge_graph").Literal("namespaces");
    }
}