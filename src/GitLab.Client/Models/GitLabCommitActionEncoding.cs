using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>How <see cref="CommitAction.Content" /> is encoded when a commit action carries file content.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabCommitActionEncoding>))]
public enum GitLabCommitActionEncoding
{
    /// <summary>The content is sent verbatim as text. GitLab's default when the field is omitted.</summary>
    [JsonStringEnumMemberName("text")] Text,

    /// <summary>The content is Base64-encoded - the only safe choice for binary files.</summary>
    [JsonStringEnumMemberName("base64")] Base64
}