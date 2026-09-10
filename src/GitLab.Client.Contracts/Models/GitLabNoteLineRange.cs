namespace GitLab.Client.Models;

/// <summary>
///     The span a multiline diff comment covers, carried inside <see cref="GitLabNotePosition.LineRange" />.
///     Omit it for a comment anchored to a single line.
/// </summary>
public sealed record GitLabNoteLineRange
{
    /// <summary>First line of the range.</summary>
    public GitLabNoteLinePosition? Start { get; init; }

    /// <summary>Last line of the range, inclusive.</summary>
    public GitLabNoteLinePosition? End { get; init; }
}