namespace GitLab.Client.Models;

/// <summary>
///     Request body for editing the text of a note inside a discussion -
///     <c>PUT /projects/:id/:noteable/:noteable_id/discussions/:discussion_id/notes/:note_id</c> and its
///     group-scoped epic counterpart.
///     <para>
///         GitLab's endpoint also accepts a <c>resolved</c> flag, but validates the two as
///         <em>exactly one of</em> <c>body</c> or <c>resolved</c> - sending both, or neither, is a 400. The
///         two payloads are therefore modelled as two request types and two methods rather than one record
///         with two optional members that is invalid in three of its four states; the resolving half is
///         <c>ResolveNoteInMergeRequestDiscussionAsync</c>, which takes a
///         <see cref="ResolveDiscussionRequest" />.
///     </para>
/// </summary>
public sealed record UpdateDiscussionNoteRequest
{
    /// <summary>The replacement content of the note. GitLab rejects a blank body.</summary>
    public required string Body { get; init; }
}