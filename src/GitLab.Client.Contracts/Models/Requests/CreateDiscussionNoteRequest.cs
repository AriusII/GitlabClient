namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for adding a reply to an existing discussion thread -
///     <c>POST /projects/:id/:noteable/:noteable_id/discussions/:discussion_id/notes</c> and its group-scoped
///     epic counterpart.
///     <para>
///         Deliberately distinct from <see cref="CreateNoteRequest" />, which posts a stand-alone comment
///         through the flat Notes API: the discussion form additionally accepts <see cref="CreatedAt" />.
///     </para>
/// </summary>
public sealed record CreateDiscussionNoteRequest
{
    /// <summary>The content of the reply.</summary>
    public required string Body { get; init; }

    /// <summary>
    ///     Back-dates the note. Requires administrator or project/group owner rights; GitLab ignores it
    ///     otherwise.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }
}