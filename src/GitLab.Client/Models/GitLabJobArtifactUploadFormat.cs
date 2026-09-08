using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The storage format of an artifact uploaded via <c>POST /jobs/:id/artifacts</c>. Outbound-only,
///     like <see cref="GitLabJobArtifactUploadType" />: GitLab validates it against this closed
///     vocabulary and never reports it back on a job.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabJobArtifactUploadFormat>))]
public enum GitLabJobArtifactUploadFormat
{
    [JsonStringEnumMemberName("raw")] Raw,

    [JsonStringEnumMemberName("zip")] Zip,

    [JsonStringEnumMemberName("gzip")] Gzip
}