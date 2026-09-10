using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>sort</c> direction of the GitLab activity feeds, applied to the event's creation date.
///     GitLab defaults to <see cref="Desc" /> when the parameter is omitted.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabEventSort>))]
public enum GitLabEventSort
{
    [JsonStringEnumMemberName("asc")] Asc,

    [JsonStringEnumMemberName("desc")] Desc
}