using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Award emoji resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         The path segment is <c>award_emoji</c> - singular - even for the collection endpoints. Epics
///         are the one awardable rooted at a group rather than a project, and snippets are the one
///         awardable addressed by id rather than by iid.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IAwardEmojiService), typeof(IAwardEmojiClient))]
internal interface IAwardEmojiRepository
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