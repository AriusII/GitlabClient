using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     How a <see cref="GitLabBroadcastMessage" /> is presented to users - a persistent banner or a
///     dismissable notification. Used on <see cref="CreateBroadcastMessageRequest.BroadcastType" /> and
///     <see cref="UpdateBroadcastMessageRequest.BroadcastType" />; GitLab defaults to <see cref="Banner" />
///     when the request omits it.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabBroadcastMessageType>))]
public enum GitLabBroadcastMessageType
{
    [JsonStringEnumMemberName("banner")] Banner,

    [JsonStringEnumMemberName("notification")]
    Notification
}