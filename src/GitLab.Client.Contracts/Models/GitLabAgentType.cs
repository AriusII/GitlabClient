using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The external coding agent an identity or session belongs to
///     (<c>/projects/:id/ai_agent/identities</c>, <c>/projects/:id/ai_agent/sessions</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabAgentType>))]
public enum GitLabAgentType
{
    [JsonStringEnumMemberName("claude-code")]
    ClaudeCode,

    [JsonStringEnumMemberName("opencode")] OpenCode
}