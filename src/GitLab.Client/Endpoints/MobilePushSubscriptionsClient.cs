using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class MobilePushSubscriptionsClient(IGitLabApiConnection connection)
    : IMobilePushSubscriptionsClient
{
    public Task<GitLabMobilePushSubscription> RegisterAsync(RegisterMobilePushSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("user").Literal("push_subscriptions").Build(),
            request,
            GitLabJsonContext.Default.RegisterMobilePushSubscriptionRequest,
            GitLabJsonContext.Default.GitLabMobilePushSubscription,
            cancellationToken);
    }

    public Task UnregisterAsync(string deviceToken, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("user").Literal("push_subscriptions").Query("device_token", deviceToken)
                .Build(),
            cancellationToken);
    }
}