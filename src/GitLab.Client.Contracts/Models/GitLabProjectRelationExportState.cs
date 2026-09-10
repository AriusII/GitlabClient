using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     How far along a project relations export is
///     (<c>GET /projects/:id/export_relations/status</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabProjectRelationExportState>))]
public enum GitLabProjectRelationExportState
{
    /// <summary>Scheduled, not started.</summary>
    [JsonStringEnumMemberName("pending")] Pending,

    /// <summary>GitLab is writing the relation's NDJSON file.</summary>
    [JsonStringEnumMemberName("started")] Started,

    /// <summary>The relation can be downloaded.</summary>
    [JsonStringEnumMemberName("finished")] Finished,

    /// <summary>The export failed; see <see cref="GitLabProjectRelationExportStatus.Error" />.</summary>
    [JsonStringEnumMemberName("failed")] Failed
}