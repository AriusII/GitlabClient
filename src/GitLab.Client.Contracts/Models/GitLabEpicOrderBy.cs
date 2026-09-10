using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>Sort field accepted by a group epic listing.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabEpicOrderBy>))]
public enum GitLabEpicOrderBy
{
    [JsonStringEnumMemberName("created_at")]
    CreatedAt,

    [JsonStringEnumMemberName("updated_at")]
    UpdatedAt,

    [JsonStringEnumMemberName("title")] Title
}