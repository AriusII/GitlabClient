namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/issues/:issue_iid/discussions</c>,
///     <c>POST /projects/:id/merge_requests/:merge_request_iid/discussions</c>,
///     <c>POST /projects/:id/repository/commits/:sha/discussions</c>,
///     <c>POST /projects/:id/snippets/:snippet_id/discussions</c> and
///     <c>POST /groups/:id/epics/:epic_iid/discussions</c>.
/// </summary>
public sealed record CreateDiscussionRequest
{
    /// <summary>The content of the first note in the new discussion.</summary>
    public required string Body { get; init; }

    /// <summary>
    ///     Back-dates the note. Requires administrator or project/group owner rights; GitLab ignores it
    ///     otherwise.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    ///     Anchors the thread to a line in a diff, turning it into a review comment. Accepted only on the
    ///     merge request and commit routes - GitLab rejects it on issues, snippets and epics, which have no
    ///     diff to anchor to. Left unset, the thread is a plain comment on the noteable.
    /// </summary>
    public GitLabNotePosition? Position { get; init; }
}