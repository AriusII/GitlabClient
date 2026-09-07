using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Active context resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IActiveContextService), typeof(IActiveContextClient))]
internal interface IActiveContextRepository
{
    Task<GitLabActiveContextCodeEnabledNamespace> UpdateEnabledNamespaceStateAsync(
        UpdateActiveContextEnabledNamespaceStateRequest request, CancellationToken cancellationToken = default);

    Task<GitLabActiveContextCollectionDetail> UpdateCollectionAsync(string id,
        UpdateActiveContextCollectionRequest? request = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GitLabActiveContextConnection>> ListConnectionsAsync(
        CancellationToken cancellationToken = default);

    Task<GitLabActiveContextConnection> ActivateConnectionAsync(ActivateActiveContextConnectionRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabActiveContextConnection> DeactivateConnectionAsync(
        DeactivateActiveContextConnectionRequest? request = null, CancellationToken cancellationToken = default);

    Task ClearDeadQueueAsync(CancellationToken cancellationToken = default);

    Task ReplayDeadQueueAsync(ReplayActiveContextDeadQueueRequest request,
        CancellationToken cancellationToken = default);
}