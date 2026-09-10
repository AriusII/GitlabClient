using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>status</c> filter of <c>GET /projects/:id/ai_agent/sessions</c>.
/// </summary>
/// <remarks>
///     This is the filter vocabulary only. A session echoes its status back as a bare string in
///     <see cref="GitLabAgentSession.Status" />, and the terminal statuses a client may *set* are the
///     separate, smaller <see cref="GitLabAgentSessionOutcome" />.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabAgentSessionStatus>))]
public enum GitLabAgentSessionStatus
{
    [JsonStringEnumMemberName("created")] Created,

    [JsonStringEnumMemberName("running")] Running,

    [JsonStringEnumMemberName("finished")] Finished,

    [JsonStringEnumMemberName("failed")] Failed,

    [JsonStringEnumMemberName("stopped")] Stopped
}