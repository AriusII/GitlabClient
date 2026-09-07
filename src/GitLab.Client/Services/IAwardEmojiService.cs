using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Award emoji, sitting between the public <c>IAwardEmojiClient</c>
///     controller and <c>IAwardEmojiRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IAwardEmojiService
{
    IAsyncEnumerable<GitLabAwardEmoji> ListForIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> GetForIssueAsync(ProjectId projectId, long issueIid, long awardId,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> AddToIssueAsync(ProjectId projectId, long issueIid, CreateAwardEmojiRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteFromIssueAsync(ProjectId projectId, long issueIid, long awardId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAwardEmoji> ListForIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> GetForIssueNoteAsync(ProjectId projectId, long issueIid, long noteId, long awardId,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> AddToIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default);

    Task DeleteFromIssueNoteAsync(ProjectId projectId, long issueIid, long noteId, long awardId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAwardEmoji> ListForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> GetForMergeRequestAsync(ProjectId projectId, long mergeRequestIid, long awardId,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> AddToMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default);

    Task DeleteFromMergeRequestAsync(ProjectId projectId, long mergeRequestIid, long awardId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAwardEmoji> ListForMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid,
        long noteId, CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> GetForMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        long awardId, CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> AddToMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default);

    Task DeleteFromMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId, long awardId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAwardEmoji> ListForSnippetAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> GetForSnippetAsync(ProjectId projectId, long snippetId, long awardId,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> AddToSnippetAsync(ProjectId projectId, long snippetId, CreateAwardEmojiRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteFromSnippetAsync(ProjectId projectId, long snippetId, long awardId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAwardEmoji> ListForSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> GetForSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId, long awardId,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> AddToSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default);

    Task DeleteFromSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId, long awardId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAwardEmoji> ListForEpicAsync(GroupId groupId, long epicIid,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> GetForEpicAsync(GroupId groupId, long epicIid, long awardId,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> AddToEpicAsync(GroupId groupId, long epicIid, CreateAwardEmojiRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteFromEpicAsync(GroupId groupId, long epicIid, long awardId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAwardEmoji> ListForEpicNoteAsync(GroupId groupId, long epicIid, long noteId,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> GetForEpicNoteAsync(GroupId groupId, long epicIid, long noteId, long awardId,
        CancellationToken cancellationToken = default);

    Task<GitLabAwardEmoji> AddToEpicNoteAsync(GroupId groupId, long epicIid, long noteId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default);

    Task DeleteFromEpicNoteAsync(GroupId groupId, long epicIid, long noteId, long awardId,
        CancellationToken cancellationToken = default);
}