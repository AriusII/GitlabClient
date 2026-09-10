using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>State filter accepted by a group epic listing.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabEpicState>))]
public enum GitLabEpicState
{
    [JsonStringEnumMemberName("opened")] Opened,

    [JsonStringEnumMemberName("closed")] Closed,

    [JsonStringEnumMemberName("all")] All
}