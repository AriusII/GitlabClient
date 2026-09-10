using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     How far along the most recent project export is
///     (<c>GET /projects/:id/export</c>).
///     <para>
///         A closed vocabulary in the spec, so it is modelled as an enum. During
///         <see cref="RegenerationInProgress" />, GitLab retains the preceding archive for download.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabProjectExportState>))]
public enum GitLabProjectExportState
{
    /// <summary>No export has been scheduled.</summary>
    [JsonStringEnumMemberName("none")] None,

    /// <summary>The export has been scheduled but has not started yet.</summary>
    [JsonStringEnumMemberName("queued")] Queued,

    /// <summary>GitLab is assembling the archive.</summary>
    [JsonStringEnumMemberName("started")] Started,

    /// <summary>The archive is ready to download.</summary>
    [JsonStringEnumMemberName("finished")] Finished,

    /// <summary>GitLab is replacing an existing downloadable archive.</summary>
    [JsonStringEnumMemberName("regeneration_in_progress")]
    RegenerationInProgress,

    /// <summary>The export failed; schedule a new one.</summary>
    [JsonStringEnumMemberName("failed")] Failed
}