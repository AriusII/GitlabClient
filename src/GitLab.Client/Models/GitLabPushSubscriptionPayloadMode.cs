using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>Whether a push notification carries the full content or only identifiers the device must fetch itself.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPushSubscriptionPayloadMode>))]
public enum GitLabPushSubscriptionPayloadMode
{
    [JsonStringEnumMemberName("full")] Full,

    [JsonStringEnumMemberName("id_only")] IdOnly
}