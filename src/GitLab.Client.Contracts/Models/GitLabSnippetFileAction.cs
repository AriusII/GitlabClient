using System.Text.Json.Serialization;

using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     What an <see cref="UpdateSnippetFileRequest" /> entry does to the file it names. GitLab enumerates
///     these four values, so they are modelled as an enum rather than free text.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabSnippetFileAction>))]
public enum GitLabSnippetFileAction
{
    /// <summary>Adds a new file at <see cref="UpdateSnippetFileRequest.FilePath" />.</summary>
    [JsonStringEnumMemberName("create")] Create,

    /// <summary>Replaces the content of an existing file.</summary>
    [JsonStringEnumMemberName("update")] Update,

    /// <summary>Removes the file at <see cref="UpdateSnippetFileRequest.FilePath" />.</summary>
    [JsonStringEnumMemberName("delete")] Delete,

    /// <summary>
    ///     Renames the file at <see cref="UpdateSnippetFileRequest.PreviousPath" /> to
    ///     <see cref="UpdateSnippetFileRequest.FilePath" />, optionally rewriting its content at the same time.
    /// </summary>
    [JsonStringEnumMemberName("move")] Move
}