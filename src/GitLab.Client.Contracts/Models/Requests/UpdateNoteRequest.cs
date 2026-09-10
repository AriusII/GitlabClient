namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/:noteable/:noteable_id/notes/:note_id</c> and its group
///     counterpart. Updating a note replaces its body outright - GitLab has no partial-body edit - so
///     <see cref="Body" /> is required even though the spec marks it optional.
///     <para>
///         GitLab's <c>confidential</c> parameter is deliberately not exposed: it was deprecated in
///         14.10 and the API no longer honours it ("No longer allowed to update confidentiality of
///         notes"), so a property here would be a setting that silently does nothing.
///     </para>
/// </summary>
public sealed record UpdateNoteRequest
{
    /// <summary>The note's new content, in GitLab Flavored Markdown.</summary>
    public required string Body { get; init; }
}