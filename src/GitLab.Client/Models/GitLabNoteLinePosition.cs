namespace GitLab.Client.Models;

/// <summary>
///     One end of a <see cref="GitLabNoteLineRange" /> - the start or the end line of a multiline diff
///     comment. Every member is optional: GitLab accepts either the opaque <see cref="LineCode" /> or the
///     <see cref="OldLine" />/<see cref="NewLine" /> pair, depending on which side of the diff the line
///     exists on.
/// </summary>
public sealed record GitLabNoteLinePosition
{
    /// <summary>
    ///     GitLab's internal identifier for a diff line, of the form
    ///     <c>&lt;file-sha&gt;_&lt;old-line&gt;_&lt;new-line&gt;</c>.
    /// </summary>
    public string? LineCode { get; init; }

    /// <summary>
    ///     Which side of the diff the line sits on - <c>new</c> for an added line, <c>old</c> for a removed
    ///     one, absent for an unchanged context line.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>Line number before the change; absent for an added line.</summary>
    public int? OldLine { get; init; }

    /// <summary>Line number after the change; absent for a removed line.</summary>
    public int? NewLine { get; init; }
}