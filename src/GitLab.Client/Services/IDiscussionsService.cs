using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Discussions, sitting between the public <c>IDiscussionsClient</c>
///     controller and <c>IDiscussionsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IDiscussionsService
{
    IAsyncEnumerable<GitLabDiscussion> ListForIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> GetForIssueAsync(ProjectId projectId, long issueIid, string discussionId,
        CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> CreateForIssueAsync(ProjectId projectId, long issueIid, CreateDiscussionRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> ResolveForIssueAsync(ProjectId projectId, long issueIid, string discussionId,
        ResolveDiscussionRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> ListNotesInIssueDiscussionAsync(ProjectId projectId, long issueIid,
        string discussionId, CancellationToken cancellationToken = default);

    Task<GitLabNote> AddNoteToIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId,
        CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    Task<GitLabNote> GetNoteInIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId,
        long noteId, CancellationToken cancellationToken = default);

    Task<GitLabNote> UpdateNoteInIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId,
        long noteId, UpdateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    Task DeleteNoteFromIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId, long noteId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDiscussion> ListForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> GetForMergeRequestAsync(ProjectId projectId, long mergeRequestIid, string discussionId,
        CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> CreateForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CreateDiscussionRequest request, CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> ResolveForMergeRequestAsync(ProjectId projectId, long mergeRequestIid, string discussionId,
        ResolveDiscussionRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> ListNotesInMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, CancellationToken cancellationToken = default);

    Task<GitLabNote> AddNoteToMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    Task<GitLabNote> GetNoteInMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, long noteId, CancellationToken cancellationToken = default);

    Task<GitLabNote> UpdateNoteInMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, long noteId, UpdateDiscussionNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> ResolveNoteInMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, long noteId, ResolveDiscussionRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteNoteFromMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid, string discussionId,
        long noteId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDiscussion> ListForCommitAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> GetForCommitAsync(ProjectId projectId, string sha, string discussionId,
        CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> CreateForCommitAsync(ProjectId projectId, string sha, CreateDiscussionRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> ListNotesInCommitDiscussionAsync(ProjectId projectId, string sha,
        string discussionId, CancellationToken cancellationToken = default);

    Task<GitLabNote> AddNoteToCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId,
        CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    Task<GitLabNote> GetNoteInCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId, long noteId,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> UpdateNoteInCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId,
        long noteId, UpdateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    Task DeleteNoteFromCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId, long noteId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDiscussion> ListForSnippetAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> GetForSnippetAsync(ProjectId projectId, long snippetId, string discussionId,
        CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> CreateForSnippetAsync(ProjectId projectId, long snippetId, CreateDiscussionRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> ListNotesInSnippetDiscussionAsync(ProjectId projectId, long snippetId,
        string discussionId, CancellationToken cancellationToken = default);

    Task<GitLabNote> AddNoteToSnippetDiscussionAsync(ProjectId projectId, long snippetId, string discussionId,
        CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    Task<GitLabNote> GetNoteInSnippetDiscussionAsync(ProjectId projectId, long snippetId, string discussionId,
        long noteId, CancellationToken cancellationToken = default);

    Task<GitLabNote> UpdateNoteInSnippetDiscussionAsync(ProjectId projectId, long snippetId, string discussionId,
        long noteId, UpdateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    Task DeleteNoteFromSnippetDiscussionAsync(ProjectId projectId, long snippetId, string discussionId, long noteId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDiscussion> ListForEpicAsync(GroupId groupId, long epicIid,
        CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> GetForEpicAsync(GroupId groupId, long epicIid, string discussionId,
        CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> CreateForEpicAsync(GroupId groupId, long epicIid, CreateDiscussionRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabDiscussion> ResolveForEpicAsync(GroupId groupId, long epicIid, string discussionId,
        ResolveDiscussionRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabNote> ListNotesInEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> AddNoteToEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId,
        CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    Task<GitLabNote> GetNoteInEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId, long noteId,
        CancellationToken cancellationToken = default);

    Task<GitLabNote> UpdateNoteInEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId, long noteId,
        UpdateDiscussionNoteRequest request, CancellationToken cancellationToken = default);

    Task DeleteNoteFromEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId, long noteId,
        CancellationToken cancellationToken = default);
}