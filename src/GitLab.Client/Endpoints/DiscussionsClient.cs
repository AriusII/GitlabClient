using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class DiscussionsClient(IGitLabApiConnection connection) : IDiscussionsClient
{
    public IAsyncEnumerable<GitLabDiscussion> ListForIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            IssueDiscussions(projectId, issueIid).Build(),
            GitLabJsonContext.Default.GitLabDiscussionArray,
            cancellationToken);
    }

    public Task<GitLabDiscussion> GetForIssueAsync(ProjectId projectId, long issueIid, string discussionId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            IssueDiscussions(projectId, issueIid).Escaped(discussionId).Build(),
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public Task<GitLabDiscussion> CreateForIssueAsync(ProjectId projectId, long issueIid,
        CreateDiscussionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            IssueDiscussions(projectId, issueIid).Build(),
            request,
            GitLabJsonContext.Default.CreateDiscussionRequest,
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public Task<GitLabDiscussion> ResolveForIssueAsync(ProjectId projectId, long issueIid, string discussionId,
        ResolveDiscussionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            IssueDiscussions(projectId, issueIid).Escaped(discussionId).Build(),
            request,
            GitLabJsonContext.Default.ResolveDiscussionRequest,
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> ListNotesInIssueDiscussionAsync(ProjectId projectId, long issueIid,
        string discussionId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            IssueDiscussions(projectId, issueIid).Escaped(discussionId).Literal("notes").Build(),
            GitLabJsonContext.Default.GitLabNoteArray,
            cancellationToken);
    }

    public Task<GitLabNote> AddNoteToIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId,
        CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            IssueDiscussions(projectId, issueIid).Escaped(discussionId).Literal("notes").Build(),
            request,
            GitLabJsonContext.Default.CreateDiscussionNoteRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> GetNoteInIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId,
        long noteId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            IssueDiscussions(projectId, issueIid).Escaped(discussionId).Literal("notes").Segment(noteId).Build(),
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> UpdateNoteInIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId,
        long noteId, UpdateDiscussionNoteRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            IssueDiscussions(projectId, issueIid).Escaped(discussionId).Literal("notes").Segment(noteId).Build(),
            request,
            GitLabJsonContext.Default.UpdateDiscussionNoteRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> ResolveNoteInIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId,
        long noteId, ResolveDiscussionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            IssueDiscussions(projectId, issueIid).Escaped(discussionId).Literal("notes").Segment(noteId).Build(),
            request,
            GitLabJsonContext.Default.ResolveDiscussionRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task DeleteNoteFromIssueDiscussionAsync(ProjectId projectId, long issueIid, string discussionId,
        long noteId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            IssueDiscussions(projectId, issueIid).Escaped(discussionId).Literal("notes").Segment(noteId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDiscussion> ListForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestDiscussions(projectId, mergeRequestIid).Build(),
            GitLabJsonContext.Default.GitLabDiscussionArray,
            cancellationToken);
    }

    public Task<GitLabDiscussion> GetForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestDiscussions(projectId, mergeRequestIid).Escaped(discussionId).Build(),
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public Task<GitLabDiscussion> CreateForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CreateDiscussionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestDiscussions(projectId, mergeRequestIid).Build(),
            request,
            GitLabJsonContext.Default.CreateDiscussionRequest,
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public Task<GitLabDiscussion> ResolveForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, ResolveDiscussionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            MergeRequestDiscussions(projectId, mergeRequestIid).Escaped(discussionId).Build(),
            request,
            GitLabJsonContext.Default.ResolveDiscussionRequest,
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> ListNotesInMergeRequestDiscussionAsync(ProjectId projectId,
        long mergeRequestIid, string discussionId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestDiscussions(projectId, mergeRequestIid).Escaped(discussionId).Literal("notes").Build(),
            GitLabJsonContext.Default.GitLabNoteArray,
            cancellationToken);
    }

    public Task<GitLabNote> AddNoteToMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestDiscussions(projectId, mergeRequestIid).Escaped(discussionId).Literal("notes").Build(),
            request,
            GitLabJsonContext.Default.CreateDiscussionNoteRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> GetNoteInMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, long noteId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestNote(projectId, mergeRequestIid, discussionId, noteId),
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> UpdateNoteInMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, long noteId, UpdateDiscussionNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            MergeRequestNote(projectId, mergeRequestIid, discussionId, noteId),
            request,
            GitLabJsonContext.Default.UpdateDiscussionNoteRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> ResolveNoteInMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, long noteId, ResolveDiscussionRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            MergeRequestNote(projectId, mergeRequestIid, discussionId, noteId),
            request,
            GitLabJsonContext.Default.ResolveDiscussionRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task DeleteNoteFromMergeRequestDiscussionAsync(ProjectId projectId, long mergeRequestIid,
        string discussionId, long noteId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            MergeRequestNote(projectId, mergeRequestIid, discussionId, noteId),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDiscussion> ListForCommitAsync(ProjectId projectId, string sha,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            CommitDiscussions(projectId, sha).Build(),
            GitLabJsonContext.Default.GitLabDiscussionArray,
            cancellationToken);
    }

    public Task<GitLabDiscussion> GetForCommitAsync(ProjectId projectId, string sha, string discussionId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            CommitDiscussions(projectId, sha).Escaped(discussionId).Build(),
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public Task<GitLabDiscussion> CreateForCommitAsync(ProjectId projectId, string sha, CreateDiscussionRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            CommitDiscussions(projectId, sha).Build(),
            request,
            GitLabJsonContext.Default.CreateDiscussionRequest,
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> ListNotesInCommitDiscussionAsync(ProjectId projectId, string sha,
        string discussionId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            CommitDiscussions(projectId, sha).Escaped(discussionId).Literal("notes").Build(),
            GitLabJsonContext.Default.GitLabNoteArray,
            cancellationToken);
    }

    public Task<GitLabNote> AddNoteToCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId,
        CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            CommitDiscussions(projectId, sha).Escaped(discussionId).Literal("notes").Build(),
            request,
            GitLabJsonContext.Default.CreateDiscussionNoteRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> GetNoteInCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId,
        long noteId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            CommitNote(projectId, sha, discussionId, noteId),
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> UpdateNoteInCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId,
        long noteId, UpdateDiscussionNoteRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            CommitNote(projectId, sha, discussionId, noteId),
            request,
            GitLabJsonContext.Default.UpdateDiscussionNoteRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> ResolveNoteInCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId,
        long noteId, ResolveDiscussionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            CommitNote(projectId, sha, discussionId, noteId),
            request,
            GitLabJsonContext.Default.ResolveDiscussionRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task DeleteNoteFromCommitDiscussionAsync(ProjectId projectId, string sha, string discussionId, long noteId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            CommitNote(projectId, sha, discussionId, noteId),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDiscussion> ListForSnippetAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            SnippetDiscussions(projectId, snippetId).Build(),
            GitLabJsonContext.Default.GitLabDiscussionArray,
            cancellationToken);
    }

    public Task<GitLabDiscussion> GetForSnippetAsync(ProjectId projectId, long snippetId, string discussionId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            SnippetDiscussions(projectId, snippetId).Escaped(discussionId).Build(),
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public Task<GitLabDiscussion> CreateForSnippetAsync(ProjectId projectId, long snippetId,
        CreateDiscussionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            SnippetDiscussions(projectId, snippetId).Build(),
            request,
            GitLabJsonContext.Default.CreateDiscussionRequest,
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> ListNotesInSnippetDiscussionAsync(ProjectId projectId, long snippetId,
        string discussionId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            SnippetDiscussions(projectId, snippetId).Escaped(discussionId).Literal("notes").Build(),
            GitLabJsonContext.Default.GitLabNoteArray,
            cancellationToken);
    }

    public Task<GitLabNote> AddNoteToSnippetDiscussionAsync(ProjectId projectId, long snippetId, string discussionId,
        CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            SnippetDiscussions(projectId, snippetId).Escaped(discussionId).Literal("notes").Build(),
            request,
            GitLabJsonContext.Default.CreateDiscussionNoteRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> GetNoteInSnippetDiscussionAsync(ProjectId projectId, long snippetId, string discussionId,
        long noteId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            SnippetNote(projectId, snippetId, discussionId, noteId),
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> UpdateNoteInSnippetDiscussionAsync(ProjectId projectId, long snippetId,
        string discussionId, long noteId, UpdateDiscussionNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            SnippetNote(projectId, snippetId, discussionId, noteId),
            request,
            GitLabJsonContext.Default.UpdateDiscussionNoteRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> ResolveNoteInSnippetDiscussionAsync(ProjectId projectId, long snippetId,
        string discussionId, long noteId, ResolveDiscussionRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            SnippetNote(projectId, snippetId, discussionId, noteId),
            request,
            GitLabJsonContext.Default.ResolveDiscussionRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task DeleteNoteFromSnippetDiscussionAsync(ProjectId projectId, long snippetId, string discussionId,
        long noteId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            SnippetNote(projectId, snippetId, discussionId, noteId),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDiscussion> ListForEpicAsync(GroupId groupId, long epicIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            EpicDiscussions(groupId, epicIid).Build(),
            GitLabJsonContext.Default.GitLabDiscussionArray,
            cancellationToken);
    }

    public Task<GitLabDiscussion> GetForEpicAsync(GroupId groupId, long epicIid, string discussionId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            EpicDiscussions(groupId, epicIid).Escaped(discussionId).Build(),
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public Task<GitLabDiscussion> CreateForEpicAsync(GroupId groupId, long epicIid, CreateDiscussionRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            EpicDiscussions(groupId, epicIid).Build(),
            request,
            GitLabJsonContext.Default.CreateDiscussionRequest,
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public Task<GitLabDiscussion> ResolveForEpicAsync(GroupId groupId, long epicIid, string discussionId,
        ResolveDiscussionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            EpicDiscussions(groupId, epicIid).Escaped(discussionId).Build(),
            request,
            GitLabJsonContext.Default.ResolveDiscussionRequest,
            GitLabJsonContext.Default.GitLabDiscussion,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabNote> ListNotesInEpicDiscussionAsync(GroupId groupId, long epicIid,
        string discussionId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            EpicDiscussions(groupId, epicIid).Escaped(discussionId).Literal("notes").Build(),
            GitLabJsonContext.Default.GitLabNoteArray,
            cancellationToken);
    }

    public Task<GitLabNote> AddNoteToEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId,
        CreateDiscussionNoteRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            EpicDiscussions(groupId, epicIid).Escaped(discussionId).Literal("notes").Build(),
            request,
            GitLabJsonContext.Default.CreateDiscussionNoteRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> GetNoteInEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId,
        long noteId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            EpicNote(groupId, epicIid, discussionId, noteId),
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> UpdateNoteInEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId,
        long noteId, UpdateDiscussionNoteRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            EpicNote(groupId, epicIid, discussionId, noteId),
            request,
            GitLabJsonContext.Default.UpdateDiscussionNoteRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task<GitLabNote> ResolveNoteInEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId,
        long noteId, ResolveDiscussionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            EpicNote(groupId, epicIid, discussionId, noteId),
            request,
            GitLabJsonContext.Default.ResolveDiscussionRequest,
            GitLabJsonContext.Default.GitLabNote,
            cancellationToken);
    }

    public Task DeleteNoteFromEpicDiscussionAsync(GroupId groupId, long epicIid, string discussionId, long noteId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            EpicNote(groupId, epicIid, discussionId, noteId),
            cancellationToken);
    }

    private static GitLabRouteBuilder IssueDiscussions(ProjectId projectId, long issueIid)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("issues").Segment(issueIid)
            .Literal("discussions");
    }

    private static GitLabRouteBuilder MergeRequestDiscussions(ProjectId projectId, long mergeRequestIid)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("merge_requests")
            .Segment(mergeRequestIid).Literal("discussions");
    }

    private static Uri MergeRequestNote(ProjectId projectId, long mergeRequestIid, string discussionId, long noteId)
    {
        return MergeRequestDiscussions(projectId, mergeRequestIid).Escaped(discussionId).Literal("notes")
            .Segment(noteId).Build();
    }

    private static GitLabRouteBuilder CommitDiscussions(ProjectId projectId, string sha)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("repository").Literal("commits")
            .Escaped(sha).Literal("discussions");
    }

    private static Uri CommitNote(ProjectId projectId, string sha, string discussionId, long noteId)
    {
        return CommitDiscussions(projectId, sha).Escaped(discussionId).Literal("notes").Segment(noteId).Build();
    }

    private static GitLabRouteBuilder SnippetDiscussions(ProjectId projectId, long snippetId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("snippets").Segment(snippetId)
            .Literal("discussions");
    }

    private static Uri SnippetNote(ProjectId projectId, long snippetId, string discussionId, long noteId)
    {
        return SnippetDiscussions(projectId, snippetId).Escaped(discussionId).Literal("notes").Segment(noteId).Build();
    }

    private static GitLabRouteBuilder EpicDiscussions(GroupId groupId, long epicIid)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("epics").Segment(epicIid)
            .Literal("discussions");
    }

    private static Uri EpicNote(GroupId groupId, long epicIid, string discussionId, long noteId)
    {
        return EpicDiscussions(groupId, epicIid).Escaped(discussionId).Literal("notes").Segment(noteId).Build();
    }
}