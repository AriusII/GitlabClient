using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     How far along an import from an external forge is - the <c>import_status</c> of
///     <see cref="GitLabRemoteImportedProject" />, as returned by <c>POST /import/bitbucket</c> and
///     <c>POST /import/github/cancel</c>.
/// </summary>
/// <remarks>
///     Distinct from <see cref="GitLabProjectImportStatus.ImportStatus" />, which the spec leaves as a
///     bare string. Here the spec does enumerate the vocabulary, so it is modelled as an enum.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabRemoteImportState>))]
public enum GitLabRemoteImportState
{
    /// <summary>Queued, not started.</summary>
    [JsonStringEnumMemberName("scheduled")]
    Scheduled,

    /// <summary>GitLab is pulling from the source forge.</summary>
    [JsonStringEnumMemberName("started")] Started,

    /// <summary>The import completed.</summary>
    [JsonStringEnumMemberName("finished")] Finished,

    /// <summary>The import failed; see <see cref="GitLabRemoteImportedProject.ImportError" />.</summary>
    [JsonStringEnumMemberName("failed")] Failed,

    /// <summary>The import was cancelled, either by a user or by <c>POST /import/github/cancel</c>.</summary>
    [JsonStringEnumMemberName("canceled")] Canceled
}