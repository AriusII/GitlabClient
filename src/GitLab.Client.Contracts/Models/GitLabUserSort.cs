using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>sort</c> direction of <c>GET /users</c>, applied to
///     <see cref="UserListOptions.OrderBy" />. GitLab defaults to <see cref="Desc" />.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabUserSort>))]
public enum GitLabUserSort
{
    [JsonStringEnumMemberName("asc")] Asc,

    [JsonStringEnumMemberName("desc")] Desc
}