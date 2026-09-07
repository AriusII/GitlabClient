namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /user/push_subscriptions</c>. Registration is idempotent: re-registering
///     an existing <see cref="DeviceToken" /> refreshes its attributes, and a token previously owned by
///     another user is reassigned to the calling user.
/// </summary>
public sealed record RegisterMobilePushSubscriptionRequest
{
    /// <summary>The hexadecimal APNs device token.</summary>
    public required string DeviceToken { get; init; }

    public GitLabMobileDevicePlatform? Platform { get; init; }

    /// <summary>Defaults to <see cref="GitLabPushSubscriptionApnsEnvironment.Production" /> when omitted.</summary>
    public GitLabPushSubscriptionApnsEnvironment? ApnsEnvironment { get; init; }

    /// <summary>The application bundle identifier.</summary>
    public string? BundleId { get; init; }

    /// <summary>A human-readable device name.</summary>
    public string? DeviceName { get; init; }

    /// <summary>The installed application version.</summary>
    public string? AppVersion { get; init; }

    /// <summary>The device locale.</summary>
    public string? Locale { get; init; }

    public GitLabPushSubscriptionPayloadMode? PayloadMode { get; init; }
}