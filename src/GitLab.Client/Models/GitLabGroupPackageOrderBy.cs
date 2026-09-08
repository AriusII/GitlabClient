using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>order_by</c> field for <c>GET /groups/:id/packages</c> (<see cref="GroupPackageListOptions.OrderBy" />) -
///     a superset of <see cref="GitLabPackageOrderBy" /> that adds <see cref="ProjectPath" />, since a group
///     listing spans every project underneath it and the project route has no such column to sort by.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabGroupPackageOrderBy>))]
public enum GitLabGroupPackageOrderBy
{
    [JsonStringEnumMemberName("created_at")]
    CreatedAt,

    [JsonStringEnumMemberName("name")] Name,

    [JsonStringEnumMemberName("version")] Version,

    [JsonStringEnumMemberName("type")] Type,

    [JsonStringEnumMemberName("project_path")]
    ProjectPath
}