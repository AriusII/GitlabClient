using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>sort</c> direction of <c>GET /projects/:id/packages/:package_id/package_files</c>, applied
///     to <see cref="PackageFileListOptions.OrderBy" />. GitLab defaults to <see cref="Asc" />.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPackageFileSort>))]
public enum GitLabPackageFileSort
{
    [JsonStringEnumMemberName("asc")] Asc,

    [JsonStringEnumMemberName("desc")] Desc
}