namespace GitLab.Client.Models;

/// <summary>
///     The record created by registering a device for push notifications
///     (<c>POST /user/push_subscriptions</c>).
/// </summary>
public sealed record GitLabMobilePushSubscription
{
    public required long Id { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}