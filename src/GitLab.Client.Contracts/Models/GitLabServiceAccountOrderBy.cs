using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The attribute the service account listings sort by (<c>order_by</c>). GitLab enumerates exactly these
///     two values and rejects anything else with a 400.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabServiceAccountOrderBy>))]
public enum GitLabServiceAccountOrderBy
{
    [JsonStringEnumMemberName("id")] Id,

    [JsonStringEnumMemberName("username")] Username
}