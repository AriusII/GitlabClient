using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>State transition accepted by a legacy epic update.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabEpicStateEvent>))]
public enum GitLabEpicStateEvent
{
    [JsonStringEnumMemberName("close")] Close,

    [JsonStringEnumMemberName("reopen")] Reopen
}