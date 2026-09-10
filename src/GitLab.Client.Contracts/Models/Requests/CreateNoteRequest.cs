namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for every note-creating endpoint - <c>POST /projects/:id/:noteable/:noteable_id/notes</c>,
///     its group counterpart, and the discussion-reply routes. GitLab uses one schema for all of them.
///     <para>
///         GitLab's <c>confidential</c> parameter is deliberately not exposed: it was deprecated in 15.5
///         and renamed to <see cref="Internal" />, which is the parameter the API actually reads.
///     </para>
/// </summary>
public sealed record CreateNoteRequest
{
    /// <summary>The note's content, in GitLab Flavored Markdown.</summary>
    public required string Body { get; init; }

    /// <summary>
    ///     Creates the note as an internal note, visible only to project members. GitLab defaults to
    ///     <see langword="false" />.
    /// </summary>
    public bool? Internal { get; init; }

    /// <summary>
    ///     Backdates the note. Administrators and project owners only; GitLab silently uses "now" for
    ///     anyone else.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    ///     SHA of the head commit the note was written against, for merge request notes. GitLab rejects
    ///     the note with a <c>409</c> if the merge request has moved on since, which is what makes an
    ///     out-of-date review comment fail loudly instead of landing on the wrong diff.
    /// </summary>
    public string? MergeRequestDiffHeadSha { get; init; }
}