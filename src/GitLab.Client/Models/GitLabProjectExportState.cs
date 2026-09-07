using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     How far along the most recent project export is
///     (<c>GET /projects/:id/export</c>).
///     <para>
///         A closed vocabulary in the spec, so it is modelled as an enum. Only
///         <see cref="Finished" /> means <c>GET /projects/:id/export/download</c> will return an archive;
///         every other value answers <c>404</c> there.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabProjectExportState>))]
public enum GitLabProjectExportState
{
    /// <summary>The export has been scheduled but has not started yet.</summary>
    [JsonStringEnumMemberName("queued")] Queued,

    /// <summary>GitLab is assembling the archive.</summary>
    [JsonStringEnumMemberName("started")] Started,

    /// <summary>The archive is ready to download.</summary>
    [JsonStringEnumMemberName("finished")] Finished,

    /// <summary>The export failed; schedule a new one.</summary>
    [JsonStringEnumMemberName("failed")] Failed
}