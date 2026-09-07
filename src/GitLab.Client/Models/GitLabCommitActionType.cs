using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     What one entry of a multi-file commit's <c>actions</c> array does to a path
///     (<c>POST /projects/:id/repository/commits</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabCommitActionType>))]
public enum GitLabCommitActionType
{
    /// <summary>Adds a new file. Fails if the path already exists.</summary>
    [JsonStringEnumMemberName("create")] Create,

    /// <summary>Removes an existing file.</summary>
    [JsonStringEnumMemberName("delete")] Delete,

    /// <summary>
    ///     Renames a file. Requires <see cref="CommitAction.PreviousPath" />; content is carried over from
    ///     the old path when <see cref="CommitAction.Content" /> is omitted.
    /// </summary>
    [JsonStringEnumMemberName("move")] Move,

    /// <summary>Replaces the content of an existing file.</summary>
    [JsonStringEnumMemberName("update")] Update,

    /// <summary>
    ///     Changes only the executable bit, driven by <see cref="CommitAction.ExecuteFilemode" />; the
    ///     file's content is left alone.
    /// </summary>
    [JsonStringEnumMemberName("chmod")] Chmod
}