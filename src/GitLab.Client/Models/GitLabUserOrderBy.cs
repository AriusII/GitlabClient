using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The fields <c>GET /users</c> can be ordered by. GitLab defaults to <see cref="Id" /> when the
///     parameter is omitted.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabUserOrderBy>))]
public enum GitLabUserOrderBy
{
    [JsonStringEnumMemberName("id")] Id,

    [JsonStringEnumMemberName("name")] Name,

    [JsonStringEnumMemberName("username")] Username,

    [JsonStringEnumMemberName("created_at")]
    CreatedAt,

    [JsonStringEnumMemberName("updated_at")]
    UpdatedAt
}