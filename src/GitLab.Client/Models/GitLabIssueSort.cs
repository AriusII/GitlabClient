using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>sort</c> direction of the issue listings, applied to whatever
///     <see cref="GitLabIssueOrderBy" /> selects. GitLab defaults to <see cref="Desc" />.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabIssueSort>))]
public enum GitLabIssueSort
{
    [JsonStringEnumMemberName("asc")] Asc,

    [JsonStringEnumMemberName("desc")] Desc
}