using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class AwardEmojiClient(IGitLabApiConnection connection) : IAwardEmojiClient
{
    public IAsyncEnumerable<GitLabAwardEmoji> ListForIssueAsync(ProjectId projectId, long issueIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            IssueAwards(projectId, issueIid).Build(),
            GitLabJsonContext.Default.GitLabAwardEmojiArray,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> GetForIssueAsync(ProjectId projectId, long issueIid, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            IssueAwards(projectId, issueIid).Segment(awardId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> AddToIssueAsync(ProjectId projectId, long issueIid, CreateAwardEmojiRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            IssueAwards(projectId, issueIid).Build(),
            request,
            GitLabJsonContext.Default.CreateAwardEmojiRequest,
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task DeleteFromIssueAsync(ProjectId projectId, long issueIid, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            IssueAwards(projectId, issueIid).Segment(awardId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAwardEmoji> ListForIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            IssueNoteAwards(projectId, issueIid, noteId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmojiArray,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> GetForIssueNoteAsync(ProjectId projectId, long issueIid, long noteId, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            IssueNoteAwards(projectId, issueIid, noteId).Segment(awardId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> AddToIssueNoteAsync(ProjectId projectId, long issueIid, long noteId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            IssueNoteAwards(projectId, issueIid, noteId).Build(),
            request,
            GitLabJsonContext.Default.CreateAwardEmojiRequest,
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task DeleteFromIssueNoteAsync(ProjectId projectId, long issueIid, long noteId, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            IssueNoteAwards(projectId, issueIid, noteId).Segment(awardId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAwardEmoji> ListForMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestAwards(projectId, mergeRequestIid).Build(),
            GitLabJsonContext.Default.GitLabAwardEmojiArray,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> GetForMergeRequestAsync(ProjectId projectId, long mergeRequestIid, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestAwards(projectId, mergeRequestIid).Segment(awardId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> AddToMergeRequestAsync(ProjectId projectId, long mergeRequestIid,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestAwards(projectId, mergeRequestIid).Build(),
            request,
            GitLabJsonContext.Default.CreateAwardEmojiRequest,
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task DeleteFromMergeRequestAsync(ProjectId projectId, long mergeRequestIid, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            MergeRequestAwards(projectId, mergeRequestIid).Segment(awardId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAwardEmoji> ListForMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid,
        long noteId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MergeRequestNoteAwards(projectId, mergeRequestIid, noteId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmojiArray,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> GetForMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        long awardId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MergeRequestNoteAwards(projectId, mergeRequestIid, noteId).Segment(awardId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> AddToMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MergeRequestNoteAwards(projectId, mergeRequestIid, noteId).Build(),
            request,
            GitLabJsonContext.Default.CreateAwardEmojiRequest,
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task DeleteFromMergeRequestNoteAsync(ProjectId projectId, long mergeRequestIid, long noteId, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            MergeRequestNoteAwards(projectId, mergeRequestIid, noteId).Segment(awardId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAwardEmoji> ListForSnippetAsync(ProjectId projectId, long snippetId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            SnippetAwards(projectId, snippetId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmojiArray,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> GetForSnippetAsync(ProjectId projectId, long snippetId, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            SnippetAwards(projectId, snippetId).Segment(awardId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> AddToSnippetAsync(ProjectId projectId, long snippetId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            SnippetAwards(projectId, snippetId).Build(),
            request,
            GitLabJsonContext.Default.CreateAwardEmojiRequest,
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task DeleteFromSnippetAsync(ProjectId projectId, long snippetId, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            SnippetAwards(projectId, snippetId).Segment(awardId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAwardEmoji> ListForSnippetNoteAsync(ProjectId projectId, long snippetId,
        long noteId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            SnippetNoteAwards(projectId, snippetId, noteId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmojiArray,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> GetForSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        long awardId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            SnippetNoteAwards(projectId, snippetId, noteId).Segment(awardId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> AddToSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            SnippetNoteAwards(projectId, snippetId, noteId).Build(),
            request,
            GitLabJsonContext.Default.CreateAwardEmojiRequest,
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task DeleteFromSnippetNoteAsync(ProjectId projectId, long snippetId, long noteId, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            SnippetNoteAwards(projectId, snippetId, noteId).Segment(awardId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAwardEmoji> ListForEpicAsync(GroupId groupId, long epicIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            EpicAwards(groupId, epicIid).Build(),
            GitLabJsonContext.Default.GitLabAwardEmojiArray,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> GetForEpicAsync(GroupId groupId, long epicIid, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            EpicAwards(groupId, epicIid).Segment(awardId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> AddToEpicAsync(GroupId groupId, long epicIid, CreateAwardEmojiRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            EpicAwards(groupId, epicIid).Build(),
            request,
            GitLabJsonContext.Default.CreateAwardEmojiRequest,
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task DeleteFromEpicAsync(GroupId groupId, long epicIid, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            EpicAwards(groupId, epicIid).Segment(awardId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAwardEmoji> ListForEpicNoteAsync(GroupId groupId, long epicIid, long noteId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            EpicNoteAwards(groupId, epicIid, noteId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmojiArray,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> GetForEpicNoteAsync(GroupId groupId, long epicIid, long noteId, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            EpicNoteAwards(groupId, epicIid, noteId).Segment(awardId).Build(),
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task<GitLabAwardEmoji> AddToEpicNoteAsync(GroupId groupId, long epicIid, long noteId,
        CreateAwardEmojiRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            EpicNoteAwards(groupId, epicIid, noteId).Build(),
            request,
            GitLabJsonContext.Default.CreateAwardEmojiRequest,
            GitLabJsonContext.Default.GitLabAwardEmoji,
            cancellationToken);
    }

    public Task DeleteFromEpicNoteAsync(GroupId groupId, long epicIid, long noteId, long awardId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            EpicNoteAwards(groupId, epicIid, noteId).Segment(awardId).Build(),
            cancellationToken);
    }

    // The tag is eight identical quartets - four awardables, each with a second quartet over its notes -
    // so the route prefixes are factored out rather than repeated thirty-two times: one place per route
    // family to get the (singular) "award_emoji" segment right, instead of thirty-two chances to typo it
    // into "award_emojis" and ship a 404.
    private static GitLabRouteBuilder IssueAwards(ProjectId projectId, long issueIid)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("issues")
            .Segment(issueIid)
            .Literal("award_emoji");
    }

    private static GitLabRouteBuilder IssueNoteAwards(ProjectId projectId, long issueIid, long noteId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("issues")
            .Segment(issueIid)
            .Literal("notes")
            .Segment(noteId)
            .Literal("award_emoji");
    }

    private static GitLabRouteBuilder MergeRequestAwards(ProjectId projectId, long mergeRequestIid)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("merge_requests")
            .Segment(mergeRequestIid)
            .Literal("award_emoji");
    }

    private static GitLabRouteBuilder MergeRequestNoteAwards(ProjectId projectId, long mergeRequestIid, long noteId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("merge_requests")
            .Segment(mergeRequestIid)
            .Literal("notes")
            .Segment(noteId)
            .Literal("award_emoji");
    }

    private static GitLabRouteBuilder SnippetAwards(ProjectId projectId, long snippetId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("snippets")
            .Segment(snippetId)
            .Literal("award_emoji");
    }

    private static GitLabRouteBuilder SnippetNoteAwards(ProjectId projectId, long snippetId, long noteId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("snippets")
            .Segment(snippetId)
            .Literal("notes")
            .Segment(noteId)
            .Literal("award_emoji");
    }

    // Epics hang off a GROUP, not a project - the only awardable in this tag that does.
    private static GitLabRouteBuilder EpicAwards(GroupId groupId, long epicIid)
    {
        return GitLabRouteBuilder.Create("groups")
            .Segment(groupId)
            .Literal("epics")
            .Segment(epicIid)
            .Literal("award_emoji");
    }

    private static GitLabRouteBuilder EpicNoteAwards(GroupId groupId, long epicIid, long noteId)
    {
        return GitLabRouteBuilder.Create("groups")
            .Segment(groupId)
            .Literal("epics")
            .Segment(epicIid)
            .Literal("notes")
            .Segment(noteId)
            .Literal("award_emoji");
    }
}