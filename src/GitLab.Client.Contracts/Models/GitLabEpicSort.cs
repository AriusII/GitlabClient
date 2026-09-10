using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>Sort direction accepted by a group epic listing.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabEpicSort>))]
public enum GitLabEpicSort
{
    [JsonStringEnumMemberName("asc")] Asc,

    [JsonStringEnumMemberName("desc")] Desc
}