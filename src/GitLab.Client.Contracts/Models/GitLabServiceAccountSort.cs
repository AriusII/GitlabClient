using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>sort</c> direction of the service account listings, applied to
///     <see cref="ServiceAccountListOptions.OrderBy" />. GitLab defaults to <see cref="Desc" /> when the
///     parameter is omitted.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabServiceAccountSort>))]
public enum GitLabServiceAccountSort
{
    [JsonStringEnumMemberName("asc")] Asc,

    [JsonStringEnumMemberName("desc")] Desc
}