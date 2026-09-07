using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The mobile platform a <see cref="RegisterMobilePushSubscriptionRequest" /> registers a device
///     token for. GitLab's spec enumerates a single current value.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabMobileDevicePlatform>))]
public enum GitLabMobileDevicePlatform
{
    [JsonStringEnumMemberName("ios")] Ios
}