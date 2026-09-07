using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Mobile push subscriptions, sitting between the public
///     <c>IMobilePushSubscriptionsClient</c> controller and <c>IMobilePushSubscriptionsRepository</c>'s
///     raw GitLab access. Mirrors the repository's method shapes 1:1 today (its implementation is
///     generated); this is the seam where request validation, caching, or cross-resource composition
///     would go once the resource needs more than pass-through.
/// </summary>
internal interface IMobilePushSubscriptionsService
{
    Task<GitLabMobilePushSubscription> RegisterAsync(RegisterMobilePushSubscriptionRequest request,
        CancellationToken cancellationToken = default);

    Task UnregisterAsync(string deviceToken, CancellationToken cancellationToken = default);
}