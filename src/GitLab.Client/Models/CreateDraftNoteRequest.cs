namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/merge_requests/:merge_request_iid/draft_notes</c>.
///     <para>
///         GitLab's <c>position</c> property (diff-anchored draft notes) is deliberately absent: the OpenAPI
///         document declares it as a bare <c>type: object</c> with no properties, so no source-generated DTO
///         can be derived from it.
///     </para>
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
}