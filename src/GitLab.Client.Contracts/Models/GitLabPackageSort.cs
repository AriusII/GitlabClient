using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>sort</c> direction for listing packages in the cross-format registry summary
///     (<see cref="PackageListOptions.Sort" />, <see cref="GroupPackageListOptions.Sort" />).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPackageSort>))]
public enum GitLabPackageSort
{
    [JsonStringEnumMemberName("asc")] Asc,

    [JsonStringEnumMemberName("desc")] Desc
}