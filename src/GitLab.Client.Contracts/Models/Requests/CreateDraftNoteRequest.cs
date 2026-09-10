namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/merge_requests/:merge_request_iid/draft_notes</c>.
/// </summary>
public sealed record CreateDraftNoteRequest
{
    /// <summary>The comment text. GitLab spells this member <c>note</c> here, not <c>body</c> as elsewhere.</summary>
    public required string Note { get; init; }

    /// <summary>Set to reply into an existing discussion instead of starting a new one.</summary>
    public string? InReplyToDiscussionId { get; init; }

    /// <summary>SHA of the commit to associate the draft note with.</summary>
    public string? CommitId { get; init; }

    /// <summary>Whether publishing this note should also resolve the discussion it replies to.</summary>
    public bool? ResolveDiscussion { get; init; }

    /// <summary>
    ///     Anchors the draft note to a line, file, or image in the merge request diff. Leave unset to create
    ///     an unanchored draft note.
    /// </summary>
    public GitLabNotePosition? Position { get; init; }
}