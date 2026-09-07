using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The Apple Push Notification service environment a device token was issued for. GitLab defaults to
///     <see cref="Production" /> when a <see cref="RegisterMobilePushSubscriptionRequest" /> omits it.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPushSubscriptionApnsEnvironment>))]
public enum GitLabPushSubscriptionApnsEnvironment
{
    [JsonStringEnumMemberName("production")]
    Production,

    [JsonStringEnumMemberName("sandbox")] Sandbox
}