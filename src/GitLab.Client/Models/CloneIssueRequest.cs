namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/issues/:issue_iid/clone</c>. Unlike
///     <see cref="MoveIssueRequest" /> this leaves the original issue open.
/// </summary>
public sealed record CloneIssueRequest
{
    /// <summary>The numeric id of the project to clone the issue into.</summary>
    public required long ToProjectId { get; init; }

    /// <summary>Whether to carry the notes across. GitLab defaults to <see langword="true" />.</summary>
    public bool? WithNotes { get; init; }
}