using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     What one entry in a job's artifacts archive is. Unlike GitLab's open-ended status and file-type
///     fields this vocabulary is structural rather than editorial - an archive member is a file or a
///     directory and nothing else - so it is modelled as an enum.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabJobArtifactEntryType>))]
public enum GitLabJobArtifactEntryType
{
    [JsonStringEnumMemberName("file")] File,

    [JsonStringEnumMemberName("directory")]
    Directory
}