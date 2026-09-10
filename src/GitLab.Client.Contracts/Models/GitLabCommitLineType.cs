using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Which side of a diff a commit comment is anchored to
///     (<c>POST /projects/:id/repository/commits/:sha/comments</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabCommitLineType>))]
public enum GitLabCommitLineType
{
    /// <summary>The line as it exists after the commit.</summary>
    [JsonStringEnumMemberName("new")] New,

    /// <summary>The line as it existed before the commit.</summary>
    [JsonStringEnumMemberName("old")] Old
}