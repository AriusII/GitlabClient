namespace GitLab.Client.Models;

/// <summary>
///     A pending, unpublished comment on a merge request, as returned by the GitLab Draft notes API
///     (<c>/projects/:id/merge_requests/:merge_request_iid/draft_notes</c>). Draft notes are visible only
///     to their author until they are published.
/// </summary>
public sealed record GitLabDraftNote
{
    public required long Id { get; init; }

    /// <summary>
    ///     Raw user ID of the author. GitLab embeds no user object on this entity, so there is deliberately no
    ///     <c>Author</c> navigation property.
    /// </summary>
    public long? AuthorId { get; init; }

    /// <summary>Database ID of the merge request - not the IID used in routes.</summary>
    public long? MergeRequestId { get; init; }

    /// <summary>Whether publishing this note also resolves the discussion it replies to.</summary>
    public bool? ResolveDiscussion { get; init; }

    /// <summary>ID of the discussion this draft note replies into, when it is a reply rather than a new thread.</summary>
    public string? DiscussionId { get; init; }

    /// <summary>The comment text. GitLab calls this member <c>note</c> here, not <c>body</c>.</summary>
    public string? Note { get; init; }

    public string? CommitId { get; init; }

    /// <summary>GitLab's internal identifier for the diff line the note is anchored to.</summary>
    public string? LineCode { get; init; }

    /// <summary>
    ///     The diff anchor for this draft note when it is attached to a line, file, or image rather than the
    ///     merge request as a whole.
    /// </summary>
    public GitLabNotePosition? Position { get; init; }
}