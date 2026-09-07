using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The lifecycle of one relation inside a group relations export
///     (<c>GET /groups/:id/export_relations/status</c>).
/// </summary>
/// <remarks>
///     Deliberately group-scoped rather than shared with the project relations export: the two areas are
///     wrapped by separate resources and GitLab is free to grow one vocabulary without the other.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabGroupRelationExportState>))]
public enum GitLabGroupRelationExportState
{
    [JsonStringEnumMemberName("pending")] Pending,

    [JsonStringEnumMemberName("started")] Started,

    [JsonStringEnumMemberName("finished")] Finished,

    [JsonStringEnumMemberName("failed")] Failed
}