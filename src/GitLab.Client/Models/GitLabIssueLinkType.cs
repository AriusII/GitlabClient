using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The relation created by <c>POST /projects/:id/issues/:issue_iid/links</c>. GitLab defaults to
///     <see cref="RelatesTo" /> when the field is omitted.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabIssueLinkType>))]
public enum GitLabIssueLinkType
{
    [JsonStringEnumMemberName("relates_to")]
    RelatesTo,

    [JsonStringEnumMemberName("blocks")] Blocks,

    [JsonStringEnumMemberName("is_blocked_by")]
    IsBlockedBy
}