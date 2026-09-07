using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Broadcast messages, sitting between the public
///     <c>IBroadcastMessagesClient</c> controller and <c>IBroadcastMessagesRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this
///     is the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface IBroadcastMessagesService
{
    IAsyncEnumerable<GitLabBroadcastMessage> ListAsync(BroadcastMessageListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabBroadcastMessage> GetAsync(long id, CancellationToken cancellationToken = default);

    Task<GitLabBroadcastMessage> CreateAsync(CreateBroadcastMessageRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabBroadcastMessage> UpdateAsync(long id, UpdateBroadcastMessageRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabBroadcastMessage> DeleteAsync(long id, CancellationToken cancellationToken = default);
}