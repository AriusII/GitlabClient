using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Draft notes" API area
///     (<c>/projects/:id/merge_requests/:merge_request_iid/draft_notes</c>) - the pending, unpublished
///     comments that make up a batched merge request review.
///     <para>
///         Draft notes are per-author and invisible to everyone else until published, so every method here
///         acts on the authenticated user's own pending notes. Publish them one at a time with
///         <see cref="PublishAsync" />, or as a single review event - optionally setting a reviewer state and
///         posting a summary comment - with <see cref="PublishAllAsync" />.
///     </para>
/// </summary>
public interface IDraftNotesClient
{
    /// <summary>Streams the current user's pending draft notes on a merge request.</summary>
    IAsyncEnumerable<GitLabDraftNote> ListAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one pending draft note.</summary>
    Task<GitLabDraftNote> GetAsync(ProjectId projectId, long mergeRequestIid, long draftNoteId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a pending draft note to a merge request without publishing it.</summary>
    Task<GitLabDraftNote> CreateAsync(ProjectId projectId, long mergeRequestIid, CreateDraftNoteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Rewrites a pending draft note and returns the updated note.</summary>
    Task<GitLabDraftNote> UpdateAsync(ProjectId projectId, long mergeRequestIid, long draftNoteId,
        UpdateDraftNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Discards a pending draft note without publishing it.</summary>
    Task DeleteAsync(ProjectId projectId, long mergeRequestIid, long draftNoteId,
        CancellationToken cancellationToken = default);

    /// <summary>Publishes a single pending draft note, turning it into a visible comment.</summary>
    Task PublishAsync(ProjectId projectId, long mergeRequestIid, long draftNoteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Publishes every pending draft note the current user holds on the merge request as one review event.
    /// </summary>
    Task PublishAllAsync(ProjectId projectId, long mergeRequestIid, PublishDraftNotesRequest request,
        CancellationToken cancellationToken = default);
}