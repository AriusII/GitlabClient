using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Whether GitLab Duo asks for approval before running a tool in a session
///     (<c>tool_approval_for_session_availability</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabGroupToolApprovalAvailability>))]
public enum GitLabGroupToolApprovalAvailability
{
    [JsonStringEnumMemberName("default_on")]
    DefaultOn,

    [JsonStringEnumMemberName("default_off")]
    DefaultOff,

    [JsonStringEnumMemberName("never_on")] NeverOn
}