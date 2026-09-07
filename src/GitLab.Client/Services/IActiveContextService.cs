using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Active context, sitting between the public
///     <c>IActiveContextClient</c> controller and <c>IActiveContextRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IActiveContextService
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