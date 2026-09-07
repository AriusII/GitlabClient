using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The color theme of a <see cref="GitLabBroadcastMessage" />. Used on
///     <see cref="CreateBroadcastMessageRequest.Theme" /> and <see cref="UpdateBroadcastMessageRequest.Theme" />.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabBroadcastMessageTheme>))]
public enum GitLabBroadcastMessageTheme
{
    [JsonStringEnumMemberName("indigo")] Indigo,

    [JsonStringEnumMemberName("light-indigo")]
    LightIndigo,

    [JsonStringEnumMemberName("blue")] Blue,

    [JsonStringEnumMemberName("light-blue")]
    LightBlue,

    [JsonStringEnumMemberName("green")] Green,

    [JsonStringEnumMemberName("light-green")]
    LightGreen,

    [JsonStringEnumMemberName("red")] Red,

    [JsonStringEnumMemberName("light-red")]
    LightRed,

    [JsonStringEnumMemberName("dark")] Dark,

    [JsonStringEnumMemberName("light")] Light
}