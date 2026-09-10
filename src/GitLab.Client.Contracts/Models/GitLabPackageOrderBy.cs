using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>order_by</c> field for <c>GET /projects/:id/packages</c> (<see cref="PackageListOptions.OrderBy" />).
///     See <see cref="GitLabGroupPackageOrderBy" /> for the group route's wider vocabulary.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPackageOrderBy>))]
public enum GitLabPackageOrderBy
{
    [JsonStringEnumMemberName("created_at")]
    CreatedAt,

    [JsonStringEnumMemberName("name")] Name,

    [JsonStringEnumMemberName("version")] Version,

    [JsonStringEnumMemberName("type")] Type
}