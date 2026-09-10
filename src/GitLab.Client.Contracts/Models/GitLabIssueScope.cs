using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>scope</c> filter of the issue listing and statistics endpoints. GitLab defaults to
///     <see cref="CreatedByMe" /> on <c>GET /issues</c> and to <see cref="All" /> on the project listing.
///     <para>
///         The spec also accepts the hyphenated spellings <c>created-by-me</c> and <c>assigned-to-me</c>;
///         they are legacy aliases of the members below and are deliberately not exposed.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabIssueScope>))]
public enum GitLabIssueScope
{
    [JsonStringEnumMemberName("created_by_me")]
    CreatedByMe,

    [JsonStringEnumMemberName("assigned_to_me")]
    AssignedToMe,

    [JsonStringEnumMemberName("all")] All
}