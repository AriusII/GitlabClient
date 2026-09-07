using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The terminal status a client may move an external agent session to with
///     <c>PATCH /projects/:id/ai_agent/sessions/:session_id</c>.
/// </summary>
/// <remarks>
///     Deliberately narrower than <see cref="GitLabAgentSessionStatus" />: GitLab accepts only these two
///     values on the update, and reserves the rest of the vocabulary for statuses it sets itself.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabAgentSessionOutcome>))]
public enum GitLabAgentSessionOutcome
{
    [JsonStringEnumMemberName("completed")]
    Completed,

    [JsonStringEnumMemberName("failed")] Failed
}