using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>sort</c> direction for <c>GET /admin/data_management/:model_name</c>. GitLab defaults to
///     <see cref="Ascending" />.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AdminModelSortDirection>))]
public enum AdminModelSortDirection
{
    [JsonStringEnumMemberName("asc")] Ascending,

    [JsonStringEnumMemberName("desc")] Descending
}