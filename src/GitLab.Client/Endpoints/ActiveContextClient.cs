using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ActiveContextClient(IGitLabApiConnection connection) : IActiveContextClient
{
    public Task<GitLabActiveContextCodeEnabledNamespace> UpdateEnabledNamespaceStateAsync(
        UpdateActiveContextEnabledNamespaceStateRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            CodeRoute().Literal("enabled_namespaces").Build(),
            request,
            GitLabJsonContext.Default.UpdateActiveContextEnabledNamespaceStateRequest,
            GitLabJsonContext.Default.GitLabActiveContextCodeEnabledNamespace,
            cancellationToken);
    }

    public Task<GitLabActiveContextCollectionDetail> UpdateCollectionAsync(string id,
        UpdateActiveContextCollectionRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ActiveContextRoute().Literal("collections").Escaped(id).Build(),
            request ?? new UpdateActiveContextCollectionRequest(),
            GitLabJsonContext.Default.UpdateActiveContextCollectionRequest,
            GitLabJsonContext.Default.GitLabActiveContextCollectionDetail,
            cancellationToken);
    }

    public async Task<IReadOnlyList<GitLabActiveContextConnection>> ListConnectionsAsync(
        CancellationToken cancellationToken = default)
    {
        // One unpaginated array of instance connections, so there is nothing for GetPagedAsync to
        // follow - the result is materialized rather than streamed.
        return await connection.GetAsync(
                ConnectionsRoute().Build(),
                GitLabJsonContext.Default.GitLabActiveContextConnectionArray,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<GitLabActiveContextConnection> ActivateConnectionAsync(
        ActivateActiveContextConnectionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ConnectionsRoute().Literal("activate").Build(),
            request,
            GitLabJsonContext.Default.ActivateActiveContextConnectionRequest,
            GitLabJsonContext.Default.GitLabActiveContextConnection,
            cancellationToken);
    }

    public Task<GitLabActiveContextConnection> DeactivateConnectionAsync(
        DeactivateActiveContextConnectionRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ConnectionsRoute().Literal("deactivate").Build(),
            request ?? new DeactivateActiveContextConnectionRequest(),
            GitLabJsonContext.Default.DeactivateActiveContextConnectionRequest,
            GitLabJsonContext.Default.GitLabActiveContextConnection,
            cancellationToken);
    }

    public Task ClearDeadQueueAsync(CancellationToken cancellationToken = default)
    {
        // GitLab answers 200 (with an undocumented "items cleared" count the spec gives no schema
        // for) rather than 204, but the no-response DeleteAsync overload works for either - it only
        // checks for a success status code and never reads the body.
        return connection.DeleteAsync(
            ActiveContextRoute().Literal("dead_queue").Build(),
            cancellationToken);
    }

    public Task ReplayDeadQueueAsync(ReplayActiveContextDeadQueueRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ActiveContextRoute().Literal("dead_queue").Literal("replay").Build(),
            request,
            GitLabJsonContext.Default.ReplayActiveContextDeadQueueRequest,
            cancellationToken);
    }

    /// <summary>
    ///     The instance scope has no id segment - there is exactly one ActiveContext configuration - so
    ///     every route shares this fixed prefix. "admin" and "active_context" come from the route
    ///     template, never from a caller.
    /// </summary>
    private static GitLabRouteBuilder ActiveContextRoute()
    {
        return GitLabRouteBuilder.Create("admin").Literal("active_context");
    }

    private static GitLabRouteBuilder ConnectionsRoute()
    {
        return ActiveContextRoute().Literal("connections");
    }

    private static GitLabRouteBuilder CodeRoute()
    {
        return ActiveContextRoute().Literal("code");
    }
}