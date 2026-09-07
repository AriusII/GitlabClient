using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>state</c> filter of the issue listings (<c>GET /issues</c>,
///     <c>GET /projects/:id/issues</c>). GitLab returns every issue when the parameter is omitted.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabIssueStateFilter>))]
public enum GitLabIssueStateFilter
{
    [JsonStringEnumMemberName("opened")] Opened,

    [JsonStringEnumMemberName("closed")] Closed,

    [JsonStringEnumMemberName("all")] All
}