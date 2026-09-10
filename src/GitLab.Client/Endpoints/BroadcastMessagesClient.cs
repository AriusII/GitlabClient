using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class BroadcastMessagesClient(IGitLabApiConnection connection) : IBroadcastMessagesClient
{
    public IAsyncEnumerable<GitLabBroadcastMessage> ListAsync(BroadcastMessageListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("broadcast_messages").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabBroadcastMessageArray,
            cancellationToken);
    }

    public Task<GitLabBroadcastMessage> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("broadcast_messages").Segment(id).Build(),
            GitLabJsonContext.Default.GitLabBroadcastMessage,
            cancellationToken);
    }

    public Task<GitLabBroadcastMessage> CreateAsync(CreateBroadcastMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("broadcast_messages").Build(),
            request,
            GitLabJsonContext.Default.CreateBroadcastMessageRequest,
            GitLabJsonContext.Default.GitLabBroadcastMessage,
            cancellationToken);
    }

    public Task<GitLabBroadcastMessage> UpdateAsync(long id, UpdateBroadcastMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("broadcast_messages").Segment(id).Build(),
            request,
            GitLabJsonContext.Default.UpdateBroadcastMessageRequest,
            GitLabJsonContext.Default.GitLabBroadcastMessage,
            cancellationToken);
    }

    public Task<GitLabBroadcastMessage> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("broadcast_messages").Segment(id).Build(),
            GitLabJsonContext.Default.GitLabBroadcastMessage,
            cancellationToken);
    }
}