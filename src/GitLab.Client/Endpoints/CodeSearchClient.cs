using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class CodeSearchClient(IGitLabApiConnection connection) : ICodeSearchClient
{
    public Task<GitLabZoektIndexedNamespace> UpdateNamespaceReplicasAsync(string id,
        UpdateZoektNamespaceReplicasRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            ZoektRoute().Literal("namespaces").Escaped(id).Build(),
            request ?? new UpdateZoektNamespaceReplicasRequest(),
            GitLabJsonContext.Default.UpdateZoektNamespaceReplicasRequest,
            GitLabJsonContext.Default.GitLabZoektIndexedNamespace,
            cancellationToken);
    }

    public Task<GitLabZoektProjectIndexResult> IndexProjectAsync(long projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ZoektRoute().Literal("projects").Segment(projectId).Literal("index").Build(),
            GitLabJsonContext.Default.GitLabZoektProjectIndexResult,
            cancellationToken);
    }

    public async Task<IReadOnlyList<GitLabZoektNode>> ListShardsAsync(CancellationToken cancellationToken = default)
    {
        // One unpaginated array of instance nodes, so there is nothing for GetPagedAsync to follow -
        // the result is materialized rather than streamed.
        return await connection.GetAsync(
                ShardsRoute().Build(),
                GitLabJsonContext.Default.GitLabZoektNodeArray,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<GitLabZoektIndexedNamespace>> ListIndexedNamespacesAsync(long nodeId,
        CancellationToken cancellationToken = default)
    {
        return await connection.GetAsync(
                ShardsRoute().Segment(nodeId).Literal("indexed_namespaces").Build(),
                GitLabJsonContext.Default.GitLabZoektIndexedNamespaceArray,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task RemoveIndexedNamespaceAsync(long nodeId, long namespaceId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ShardsRoute().Segment(nodeId).Literal("indexed_namespaces").Segment(namespaceId).Build(),
            cancellationToken);
    }

    public Task<GitLabZoektIndexedNamespace> AddIndexedNamespaceAsync(long nodeId, long namespaceId,
        AddZoektIndexedNamespaceRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ShardsRoute().Segment(nodeId).Literal("indexed_namespaces").Segment(namespaceId).Build(),
            request ?? new AddZoektIndexedNamespaceRequest(),
            GitLabJsonContext.Default.AddZoektIndexedNamespaceRequest,
            GitLabJsonContext.Default.GitLabZoektIndexedNamespace,
            cancellationToken);
    }

    /// <summary>"admin" and "zoekt" come from the route template, never from a caller.</summary>
    private static GitLabRouteBuilder ZoektRoute()
    {
        return GitLabRouteBuilder.Create("admin").Literal("zoekt");
    }

    private static GitLabRouteBuilder ShardsRoute()
    {
        return ZoektRoute().Literal("shards");
    }
}