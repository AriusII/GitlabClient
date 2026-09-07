using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's ActiveContext admin API (<c>/admin/active_context/...</c>) - the semantic-search
///     and knowledge-graph indexing pipeline's backend connections, collections and dead-letter queue.
/// </summary>
/// <remarks>
///     Every endpoint here requires instance-administrator access; GitLab answers <c>403</c> for anyone
///     else. ActiveContext is an evolving GitLab surface still under active development, so response DTOs
///     are deliberately nullable-permissive.
/// </remarks>
public interface IActiveContextClient
{
    /// <summary>Transitions one namespace's ActiveContext code indexing state to <c>pending</c> or <c>ready</c>.</summary>
    Task<GitLabActiveContextCodeEnabledNamespace> UpdateEnabledNamespaceStateAsync(
        UpdateActiveContextEnabledNamespaceStateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a collection's queue-sharding options. <paramref name="id" /> is the collection's name
    ///     or numeric ID; pass <c>null</c> for <paramref name="request" /> to send an empty update body.
    /// </summary>
    Task<GitLabActiveContextCollectionDetail> UpdateCollectionAsync(string id,
        UpdateActiveContextCollectionRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Gets every ActiveContext connection configured on this instance.</summary>
    Task<IReadOnlyList<GitLabActiveContextConnection>> ListConnectionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Activates a connection, deactivating whichever connection was previously active.</summary>
    Task<GitLabActiveContextConnection> ActivateConnectionAsync(ActivateActiveContextConnectionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deactivates a connection. This asynchronously drops the connection's indexed data and deletes
    ///     its record. Pass <c>null</c> for <paramref name="request" /> to deactivate the currently active
    ///     connection.
    /// </summary>
    Task<GitLabActiveContextConnection> DeactivateConnectionAsync(
        DeactivateActiveContextConnectionRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Removes every item from the ActiveContext dead queue.</summary>
    Task ClearDeadQueueAsync(CancellationToken cancellationToken = default);

    /// <summary>Enqueues a background task that replays dead-lettered queue items into another queue.</summary>
    Task ReplayDeadQueueAsync(ReplayActiveContextDeadQueueRequest request,
        CancellationToken cancellationToken = default);
}