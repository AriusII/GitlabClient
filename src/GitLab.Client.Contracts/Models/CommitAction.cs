namespace GitLab.Client.Models;

/// <summary>
///     One file operation inside a multi-file commit (<c>POST /projects/:id/repository/commits</c>).
///     Every action in the request is applied as part of a single commit, so the whole set either lands or
///     none of it does.
/// </summary>
public sealed record CommitAction
{
    /// <summary>What to do with <see cref="FilePath" />.</summary>
    public required GitLabCommitActionType Action { get; init; }

    /// <summary>Full path from the repository root, for example <c>src/App.cs</c>.</summary>
    public required string FilePath { get; init; }

    /// <summary>
    ///     The path being renamed away from. Required for <see cref="GitLabCommitActionType.Move" /> and
    ///     meaningless for every other action.
    /// </summary>
    public string? PreviousPath { get; init; }

    /// <summary>
    ///     The file's new content, interpreted according to <see cref="Encoding" />. Optional for
    ///     <see cref="GitLabCommitActionType.Move" /> (the existing content is kept) and for
    ///     <see cref="GitLabCommitActionType.Chmod" />.
    /// </summary>
    public string? Content { get; init; }

    /// <summary><c>text</c> or <c>base64</c>. GitLab treats content as text when this is omitted.</summary>
    public GitLabCommitActionEncoding? Encoding { get; init; }

    /// <summary>
    ///     The last known commit id for the path. GitLab rejects the action with a conflict when the file
    ///     has moved on since, which is how a read-modify-write can be made safe against concurrent edits.
    /// </summary>
    public string? LastCommitId { get; init; }

    /// <summary>
    ///     Sets or clears the executable bit. Only meaningful for
    ///     <see cref="GitLabCommitActionType.Chmod" />.
    /// </summary>
    public bool? ExecuteFilemode { get; init; }
}