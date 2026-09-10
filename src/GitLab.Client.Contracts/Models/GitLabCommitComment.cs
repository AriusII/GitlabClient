namespace GitLab.Client.Models;

/// <summary>
///     A comment left on a commit (<c>/projects/:id/repository/commits/:sha/comments</c>). A comment with
///     no <see cref="Path" /> is attached to the commit as a whole rather than to a line of its diff.
/// </summary>
public sealed record GitLabCommitComment
{
    /// <summary>The comment body, in GitLab Flavored Markdown.</summary>
    public required string Note { get; init; }

    /// <summary>The commented file's path, or <see langword="null" /> for a commit-level comment.</summary>
    public string? Path { get; init; }

    /// <summary>The commented line number within <see cref="Path" />.</summary>
    public int? Line { get; init; }

    /// <summary>
    ///     Which side of the diff <see cref="Line" /> refers to - <c>new</c> or <c>old</c>. Kept as a
    ///     string on the way back because it is GitLab's own wire vocabulary for the stored comment;
    ///     <see cref="GitLabCommitLineType" /> is the closed set the create endpoint accepts.
    /// </summary>
    public string? LineType { get; init; }

    /// <summary>Who wrote the comment.</summary>
    public GitLabUser? Author { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}