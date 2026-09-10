using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Broadcast messages" API area (<c>/broadcast_messages</c>) - instance-wide banners
///     and notifications shown in the GitLab UI. Every operation requires the Administrator role.
/// </summary>
public interface IBroadcastMessagesClient
{
    /// <summary>Streams every broadcast message on the instance, active or not.</summary>
    IAsyncEnumerable<GitLabBroadcastMessage> ListAsync(BroadcastMessageListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one broadcast message by ID.</summary>
    Task<GitLabBroadcastMessage> GetAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Creates a broadcast message.</summary>
    Task<GitLabBroadcastMessage> CreateAsync(CreateBroadcastMessageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a broadcast message. Every request member is optional; GitLab keeps the existing value for anything
    ///     omitted.
    /// </summary>
    Task<GitLabBroadcastMessage> UpdateAsync(long id, UpdateBroadcastMessageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a broadcast message, returning the message as it was just before deletion.</summary>
    Task<GitLabBroadcastMessage> DeleteAsync(long id, CancellationToken cancellationToken = default);
}