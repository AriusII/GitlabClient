using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Discussions resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         Covers all five noteables GitLab hangs discussions off - issues
///         (<c>/projects/:id/issues/:issue_iid/discussions</c>), merge requests
///         (<c>/projects/:id/merge_requests/:merge_request_iid/discussions</c>), commits
///         (<c>/projects/:id/repository/commits/:sha/discussions</c>), snippets
///         (<c>/projects/:id/snippets/:snippet_id/discussions</c>) and epics
///         (<c>/groups/:id/epics/:epic_iid/discussions</c>) - at both the thread level and the level of the
///         individual notes inside a thread. Stand-alone comments posted outside a thread stay on
///         <see cref="INotesRepository" />.
///     </para>
///     <para>
///         One deliberate departure from the OpenAPI document: it declares
///         <c>GET .../discussions/:discussion_id/notes</c> as returning a single discussion entity, which is
///         the <c>success</c> annotation on GitLab's shared Grape route block leaking into the generated
///         spec. The route presents a collection of notes - it is named <c>notes</c>, its summary is "List
///         comments in a ... discussion", and a thread-level read already exists on the route one segment
///         above it - so it is modelled here as a paged stream of <see cref="GitLabNote" />.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IDiscussionsService), typeof(IDiscussionsClient))]
internal interface IDiscussionsRepository
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