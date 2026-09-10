using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Discussions" API area (<c>/projects/:id/:noteable/:id/discussions</c>,
///     <c>/groups/:id/epics/:epic_iid/discussions</c>) - threaded, resolvable comments on issues, merge
///     requests, commits, snippets and epics.
///     <para>
///         A discussion is what <see cref="INotesClient" /> cannot express: it can be started as a thread,
///         replied into, and (for issues, merge requests and epics) resolved or reopened. Every noteable
///         also exposes the notes <em>inside</em> a thread, so a single reply can be read, edited or
///         deleted without going through the flat Notes API.
///     </para>
///     <para>
///         Commits and snippets have no thread-level resolve endpoint, so there is deliberately no
///         <c>ResolveForCommitAsync</c> or <c>ResolveForSnippetAsync</c>. Every noteable's note-level update
///         endpoint accepts <c>resolved</c>; GitLab can still reject an individual note that is not resolvable.
///     </para>
/// </summary>
public interface IDiscussionsClient
{
    /// <summary>Streams every discussion on an issue (<c>GET /projects/:id/issues/:issue_iid/discussions</c>).</summary>
    IAsyncEnumerable<GitLabDiscussion> ListForIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one issue discussion and the notes it contains.</summary>
    Task<GitLabDiscussion> GetForIssueAsync(ProjectId projectId, long issueIid, string discussionId,
        CancellationToken cancellationToken = default);

    /// <summary>Starts a new discussion thread on an issue.</summary>
    Task<GitLabDiscussion> CreateForIssueAsync(ProjectId projectId, long issueIid, CreateDiscussionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Marks an issue discussion resolved or unresolved and returns the updated thread.</summary>
    Task<GitLabDiscussion> ResolveForIssueAsync(ProjectId projectId, long issueIid, string discussionId,
        ResolveDiscussionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the notes inside one issue discussion
    ///     (<c>GET /projects/:id/issues/:issue_iid/discussions/:discussion_id/notes</c>).
    /// </summary>
    IAsyncEnumerable<GitLabNote> ListNotesInIssueDiscussionAsync(ProjectId projectId, long issueIid,
        string discussionId, CancellationToken cancellationToken = default);

    /// <summary>Adds a reply note to an existing issue discussion.</summary>
    Task<GitLabNote> AddNoteToIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId,
        CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Reads a single note inside an issue discussion.</summary>
    Task<GitLabNote> GetNoteInIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId,
        long noteId, CancellationToken cancellationToken = default);

    /// <summary>Rewrites the body of a note inside an issue discussion.</summary>
    Task<GitLabNote> UpdateNoteInIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId,
        long noteId, UpdateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Marks a note inside an issue discussion resolved or unresolved.</summary>
    Task<GitLabNote> ResolveNoteInIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId,
        long noteId, ResolveDiscussionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a single note from an issue discussion.</summary>
    Task DeleteNoteFromIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every discussion on a merge request
    ///     (<c>GET /projects/:id/merge_requests/:merge_request_iid/discussions</c>).
    /// </summary>
    IAsyncEnumerable<GitLabDiscussion> ListForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one merge request discussion and the notes it contains.</summary>
    Task<GitLabDiscussion> GetForMergeRequestAsync(ProjectId projectId, long mergeRequestIid, string discussionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts a new discussion thread on a merge request. Set
    ///     <see cref="CreateDiscussionRequest.Position" /> to anchor it to a line in the diff and make it a
    ///     review comment rather than a plain one.
    /// </summary>
    Task<GitLabDiscussion> CreateForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CreateDiscussionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Marks a merge request discussion resolved or unresolved and returns the updated thread.</summary>
    Task<GitLabDiscussion> ResolveForMergeRequestAsync(ProjectId projectId, long mergeRequestIid, string discussionId,
        ResolveDiscussionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the notes inside one merge request discussion
    ///     (<c>GET /projects/:id/merge_requests/:merge_request_iid/discussions/:discussion_id/notes</c>).
    /// </summary>
    IAsyncEnumerable<GitLabNote> ListNotesInMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, CancellationToken cancellationToken = default);

    /// <summary>Adds a reply note to an existing merge request discussion.</summary>
    Task<GitLabNote> AddNoteToMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Reads a single note inside a merge request discussion.</summary>
    Task<GitLabNote> GetNoteInMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, long noteId, CancellationToken cancellationToken = default);

    /// <summary>Rewrites the body of a note inside a merge request discussion.</summary>
    Task<GitLabNote> UpdateNoteInMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, long noteId, UpdateDiscussionNoteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Marks a note inside a merge request discussion resolved or unresolved.</summary>
    Task<GitLabNote> ResolveNoteInMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, long noteId, ResolveDiscussionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a single note from a merge request discussion.</summary>
    Task DeleteNoteFromMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid, string discussionId,
        long noteId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every discussion on a commit
    ///     (<c>GET /projects/:id/repository/commits/:sha/discussions</c>).
    /// </summary>
    IAsyncEnumerable<GitLabDiscussion> ListForCommitAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one commit discussion and the notes it contains.</summary>
    Task<GitLabDiscussion> GetForCommitAsync(ProjectId projectId, string sha, string discussionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts a new discussion thread on a commit. Accepts
    ///     <see cref="CreateDiscussionRequest.Position" /> to anchor the thread to a line of the commit's
    ///     diff.
    /// </summary>
    Task<GitLabDiscussion> CreateForCommitAsync(ProjectId projectId, string sha, CreateDiscussionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the notes inside one commit discussion
    ///     (<c>GET /projects/:id/repository/commits/:sha/discussions/:discussion_id/notes</c>).
    /// </summary>
    IAsyncEnumerable<GitLabNote> ListNotesInCommitDiscussionAsync(ProjectId projectId, string sha,
        string discussionId, CancellationToken cancellationToken = default);

    /// <summary>Adds a reply note to an existing commit discussion.</summary>
    Task<GitLabNote> AddNoteToCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId,
        CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Reads a single note inside a commit discussion.</summary>
    Task<GitLabNote> GetNoteInCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Rewrites the body of a note inside a commit discussion.</summary>
    Task<GitLabNote> UpdateNoteInCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId,
        long noteId, UpdateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Marks a note inside a commit discussion resolved or unresolved.</summary>
    Task<GitLabNote> ResolveNoteInCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId,
        long noteId, ResolveDiscussionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a single note from a commit discussion.</summary>
    Task DeleteNoteFromCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every discussion on a project snippet
    ///     (<c>GET /projects/:id/snippets/:snippet_id/discussions</c>). The snippet is addressed by its
    ///     database ID, not an IID.
    /// </summary>
    IAsyncEnumerable<GitLabDiscussion> ListForSnippetAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one snippet discussion and the notes it contains.</summary>
    Task<GitLabDiscussion> GetForSnippetAsync(ProjectId projectId, long snippetId, string discussionId,
        CancellationToken cancellationToken = default);

    /// <summary>Starts a new discussion thread on a project snippet.</summary>
    Task<GitLabDiscussion> CreateForSnippetAsync(ProjectId projectId, long snippetId, CreateDiscussionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the notes inside one snippet discussion
    ///     (<c>GET /projects/:id/snippets/:snippet_id/discussions/:discussion_id/notes</c>).
    /// </summary>
    IAsyncEnumerable<GitLabNote> ListNotesInSnippetDiscussionAsync(ProjectId projectId, long snippetId,
        string discussionId, CancellationToken cancellationToken = default);

    /// <summary>Adds a reply note to an existing snippet discussion.</summary>
    Task<GitLabNote> AddNoteToSnippetDiscussionAsync(ProjectId projectId, long snippetId, string discussionId,
        CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Reads a single note inside a snippet discussion.</summary>
    Task<GitLabNote> GetNoteInSnippetDiscussionAsync(ProjectId projectId, long snippetId, string discussionId,
        long noteId, CancellationToken cancellationToken = default);

    /// <summary>Rewrites the body of a note inside a snippet discussion.</summary>
    Task<GitLabNote> UpdateNoteInSnippetDiscussionAsync(ProjectId projectId, long snippetId, string discussionId,
        long noteId, UpdateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Marks a note inside a snippet discussion resolved or unresolved.</summary>
    Task<GitLabNote> ResolveNoteInSnippetDiscussionAsync(ProjectId projectId, long snippetId, string discussionId,
        long noteId, ResolveDiscussionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a single note from a snippet discussion.</summary>
    Task DeleteNoteFromSnippetDiscussionAsync(ProjectId projectId, long snippetId, string discussionId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every discussion on a group epic
    ///     (<c>GET /groups/:id/epics/:epic_iid/discussions</c>). Epic discussions are group-scoped, so this
    ///     is the one family here that takes a <see cref="GroupId" />.
    /// </summary>
    IAsyncEnumerable<GitLabDiscussion> ListForEpicAsync(GroupId groupId, long epicIid,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one epic discussion and the notes it contains.</summary>
    Task<GitLabDiscussion> GetForEpicAsync(GroupId groupId, long epicIid, string discussionId,
        CancellationToken cancellationToken = default);

    /// <summary>Starts a new discussion thread on an epic.</summary>
    Task<GitLabDiscussion> CreateForEpicAsync(GroupId groupId, long epicIid, CreateDiscussionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Marks an epic discussion resolved or unresolved and returns the updated thread.</summary>
    Task<GitLabDiscussion> ResolveForEpicAsync(GroupId groupId, long epicIid, string discussionId,
        ResolveDiscussionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the notes inside one epic discussion
    ///     (<c>GET /groups/:id/epics/:epic_iid/discussions/:discussion_id/notes</c>).
    /// </summary>
    IAsyncEnumerable<GitLabNote> ListNotesInEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a reply note to an existing epic discussion.</summary>
    Task<GitLabNote> AddNoteToEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId,
        CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Reads a single note inside an epic discussion.</summary>
    Task<GitLabNote> GetNoteInEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId, long noteId,
        CancellationToken cancellationToken = default);

    /// <summary>Rewrites the body of a note inside an epic discussion.</summary>
    Task<GitLabNote> UpdateNoteInEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId, long noteId,
        UpdateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Marks a note inside an epic discussion resolved or unresolved.</summary>
    Task<GitLabNote> ResolveNoteInEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId, long noteId,
        ResolveDiscussionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a single note from an epic discussion.</summary>
    Task DeleteNoteFromEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId, long noteId,
        CancellationToken cancellationToken = default);
}