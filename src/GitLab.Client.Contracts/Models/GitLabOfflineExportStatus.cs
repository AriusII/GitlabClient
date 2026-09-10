using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>status</c> filter of <c>GET /offline_exports</c>. Narrower than
///     <see cref="GitLabBulkImportStatus" />: an offline transfer export writes to object storage rather
///     than streaming from a source instance, so it has neither a <c>timeout</c> nor a <c>canceled</c>
///     state.
/// </summary>
/// <remarks>
///     This is the filter vocabulary only. An export echoes its own status back as a bare string in
///     <see cref="GitLabOfflineExport.Status" />, because the pinned spec declares no response entity for
///     these routes and a value GitLab adds later must not turn a healthy response into a
///     <c>JsonException</c>.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabOfflineExportStatus>))]
public enum GitLabOfflineExportStatus
{
    [JsonStringEnumMemberName("created")] Created,

    [JsonStringEnumMemberName("started")] Started,

    [JsonStringEnumMemberName("finished")] Finished,

    [JsonStringEnumMemberName("failed")] Failed
}