using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Broadcast messages resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IBroadcastMessagesService), typeof(IBroadcastMessagesClient))]
internal interface IBroadcastMessagesRepository
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