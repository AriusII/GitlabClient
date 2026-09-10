using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Mobile push subscriptions" API area (<c>/user/push_subscriptions</c>) - the
///     calling user's registered device tokens for GitLab's mobile push notifications.
/// </summary>
public interface IMobilePushSubscriptionsClient
{
    /// <summary>
    ///     Idempotently registers the calling user's device token for push notifications. Re-registering an
    ///     existing token refreshes its attributes; a token previously owned by another user is reassigned to
    ///     the calling user.
    /// </summary>
    Task<GitLabMobilePushSubscription> RegisterAsync(RegisterMobilePushSubscriptionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes the calling user's push subscription for the given device token.</summary>
    Task UnregisterAsync(string deviceToken, CancellationToken cancellationToken = default);
}