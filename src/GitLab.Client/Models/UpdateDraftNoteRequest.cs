namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/merge_requests/:merge_request_iid/draft_notes/:draft_note_id</c>.
///     Nothing is required; an unset member is omitted from the payload rather than sent as null, so it leaves
///     the stored value alone.
///     <para>
///         GitLab's <c>position</c> property is deliberately absent for the same reason as on
///         <see cref="CreateDraftNoteRequest" />.
///     </para>
/// </summary>
public sealed record UpdateDraftNoteRequest
{
    /// <summary>The replacement comment text.</summary>
    public string? Note { get; init; }
}