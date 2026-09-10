namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for
///     <c>POST /projects/:id/merge_requests/:merge_request_iid/draft_notes/bulk_publish</c>, which publishes
///     every pending draft note the current user holds on the merge request as one review event.
/// </summary>
public sealed record PublishDraftNotesRequest
{
    /// <summary>
    ///     Reviewer state to record after publishing: <c>requested_changes</c> or <c>reviewed</c>. GitLab's own
    ///     description notes that this does not record a formal approval.
    /// </summary>
    public GitLabReviewerState? ReviewerState { get; init; }

    /// <summary>Body of a summary note to post on the merge request alongside the published drafts.</summary>
    public string? Note { get; init; }

    /// <summary>Whether the summary note is internal, visible only to project members.</summary>
    public bool? Internal { get; init; }
}