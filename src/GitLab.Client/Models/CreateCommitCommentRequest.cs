namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/repository/commits/:sha/comments</c>. Supply
///     <see cref="Path" />, <see cref="Line" /> and <see cref="LineType" /> together to anchor the comment
///     to a line of the diff; omit all three to comment on the commit as a whole.
/// </summary>
public sealed record CreateCommitCommentRequest
{
    /// <summary>The comment body.</summary>
    public required string Note { get; init; }

    /// <summary>The file to comment on, as a full path from the repository root.</summary>
    public string? Path { get; init; }

    /// <summary>The line number within <see cref="Path" /> to comment on.</summary>
    public int? Line { get; init; }

    /// <summary>Which side of the diff <see cref="Line" /> refers to.</summary>
    public GitLabCommitLineType? LineType { get; init; }
}