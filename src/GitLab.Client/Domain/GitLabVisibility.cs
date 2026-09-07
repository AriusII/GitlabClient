using System.Text.Json.Serialization;

namespace GitLab.Client.Domain;

[JsonConverter(typeof(JsonStringEnumConverter<GitLabVisibility>))]
public enum GitLabVisibility
{
    [JsonStringEnumMemberName("private")] Private,

    [JsonStringEnumMemberName("internal")] Internal,

    [JsonStringEnumMemberName("public")] Public
}