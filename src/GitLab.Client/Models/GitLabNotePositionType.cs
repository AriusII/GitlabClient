using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     What a <see cref="GitLabNotePosition" /> anchors a diff comment to. GitLab calls this member
///     <c>position_type</c> and requires it on every position object.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabNotePositionType>))]
public enum GitLabNotePositionType
{
    /// <summary>A line in a textual diff - the usual case. Pairs with the line and path members.</summary>
    [JsonStringEnumMemberName("text")] Text,

    /// <summary>A point on an image diff. Pairs with <c>width</c>, <c>height</c>, <c>x</c> and <c>y</c>.</summary>
    [JsonStringEnumMemberName("image")] Image,

    /// <summary>A whole file rather than a line inside it.</summary>
    [JsonStringEnumMemberName("file")] File
}