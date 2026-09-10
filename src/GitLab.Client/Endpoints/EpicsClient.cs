using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class EpicsClient(IGitLabApiConnection connection) : IEpicsClient
{
    public IAsyncEnumerable<GitLabEpic> ListAsync(GroupId groupId, EpicListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            EpicRoot(groupId).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabEpicArray,
            cancellationToken);
    }

    public Task<GitLabEpic> GetAsync(GroupId groupId, long epicIid, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            EpicRoute(groupId, epicIid).Build(),
            GitLabJsonContext.Default.GitLabEpic,
            cancellationToken);
    }

    public Task<GitLabEpic> CreateAsync(GroupId groupId, CreateEpicRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            EpicRoot(groupId).Build(),
            request,
            GitLabJsonContext.Default.CreateEpicRequest,
            GitLabJsonContext.Default.GitLabEpic,
            cancellationToken);
    }

    public Task<GitLabEpic> UpdateAsync(GroupId groupId, long epicIid, UpdateEpicRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            EpicRoute(groupId, epicIid).Build(),
            request,
            GitLabJsonContext.Default.UpdateEpicRequest,
            GitLabJsonContext.Default.GitLabEpic,
            cancellationToken);
    }

    public Task DeleteAsync(GroupId groupId, long epicIid, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(EpicRoute(groupId, epicIid).Build(), cancellationToken);
    }

    public IAsyncEnumerable<GitLabEpic> ListChildEpicsAsync(GroupId groupId, long epicIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ChildEpicsRoute(groupId, epicIid).Build(),
            GitLabJsonContext.Default.GitLabEpicArray,
            cancellationToken);
    }

    public Task<GitLabEpic> CreateChildEpicAsync(GroupId groupId, long epicIid, CreateChildEpicRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ChildEpicsRoute(groupId, epicIid).Build(),
            request,
            GitLabJsonContext.Default.CreateChildEpicRequest,
            GitLabJsonContext.Default.GitLabEpic,
            cancellationToken);
    }

    public Task<GitLabEpic> RelateChildEpicAsync(GroupId groupId, long epicIid, long childEpicId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ChildEpicRoute(groupId, epicIid, childEpicId).Build(),
            GitLabJsonContext.Default.GitLabEpic,
            cancellationToken);
    }

    public Task<GitLabEpic> RemoveChildEpicAsync(GroupId groupId, long epicIid, long childEpicId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ChildEpicRoute(groupId, epicIid, childEpicId).Build(),
            GitLabJsonContext.Default.GitLabEpic,
            cancellationToken);
    }

    public Task<GitLabEpic> ReorderChildEpicAsync(GroupId groupId, long epicIid, long childEpicId,
        ReorderEpicRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ChildEpicRoute(groupId, epicIid, childEpicId).Build(),
            request,
            GitLabJsonContext.Default.ReorderEpicRequest,
            GitLabJsonContext.Default.GitLabEpic,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabEpicIssue> ListIssuesAsync(GroupId groupId, long epicIid,
        EpicIssueListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            EpicIssuesRoute(groupId, epicIid).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabEpicIssueArray,
            cancellationToken);
    }

    public Task<GitLabEpicIssueLink> AddIssueAsync(GroupId groupId, long epicIid, long issueId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            EpicIssuesRoute(groupId, epicIid).Segment(issueId).Build(),
            GitLabJsonContext.Default.GitLabEpicIssueLink,
            cancellationToken);
    }

    public Task<GitLabEpicIssueLink> RemoveIssueAsync(GroupId groupId, long epicIid, long epicIssueId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            EpicIssuesRoute(groupId, epicIid).Segment(epicIssueId).Build(),
            GitLabJsonContext.Default.GitLabEpicIssueLink,
            cancellationToken);
    }

    public Task<GitLabEpicIssue> ReorderIssueAsync(GroupId groupId, long epicIid, long epicIssueId,
        ReorderEpicIssueRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            EpicIssuesRoute(groupId, epicIid).Segment(epicIssueId).Build(),
            request,
            GitLabJsonContext.Default.ReorderEpicIssueRequest,
            GitLabJsonContext.Default.GitLabEpicIssue,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabEpicBoard> ListBoardsAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            EpicBoardsRoute(groupId).Build(),
            GitLabJsonContext.Default.GitLabEpicBoardArray,
            cancellationToken);
    }

    public Task<GitLabEpicBoard> GetBoardAsync(GroupId groupId, long boardId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            EpicBoardsRoute(groupId).Segment(boardId).Build(),
            GitLabJsonContext.Default.GitLabEpicBoard,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabEpicBoardList> ListBoardListsAsync(GroupId groupId, long boardId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            EpicBoardListsRoute(groupId, boardId).Build(),
            GitLabJsonContext.Default.GitLabEpicBoardListArray,
            cancellationToken);
    }

    public Task<GitLabEpicBoardList> GetBoardListAsync(GroupId groupId, long boardId, long listId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            EpicBoardListsRoute(groupId, boardId).Segment(listId).Build(),
            GitLabJsonContext.Default.GitLabEpicBoardList,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabRelatedEpic> ListRelatedAsync(GroupId groupId, long epicIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            EpicRoute(groupId, epicIid).Literal("related_epics").Build(),
            GitLabJsonContext.Default.GitLabRelatedEpicArray,
            cancellationToken);
    }

    public Task<GitLabRelatedEpicLink> CreateRelatedLinkAsync(GroupId groupId, long epicIid,
        CreateRelatedEpicLinkRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            EpicRoute(groupId, epicIid).Literal("related_epics").Build(),
            request,
            GitLabJsonContext.Default.CreateRelatedEpicLinkRequest,
            GitLabJsonContext.Default.GitLabRelatedEpicLink,
            cancellationToken);
    }

    public Task<GitLabRelatedEpicLink> DeleteRelatedLinkAsync(GroupId groupId, long epicIid,
        long relatedEpicLinkId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            EpicRoute(groupId, epicIid).Literal("related_epics").Segment(relatedEpicLinkId).Build(),
            GitLabJsonContext.Default.GitLabRelatedEpicLink,
            cancellationToken);
    }

    public Task<GitLabTodo> CreateTodoAsync(GroupId groupId, long epicIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            EpicRoute(groupId, epicIid).Literal("todo").Build(),
            GitLabJsonContext.Default.GitLabTodo,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabRelatedEpic> ListRelatedForGroupAsync(GroupId groupId,
        RelatedEpicLinkListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("related_epic_links")
                .QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabRelatedEpicArray,
            cancellationToken);
    }

    private static GitLabRouteBuilder EpicRoot(GroupId groupId)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("epics");
    }

    private static GitLabRouteBuilder EpicRoute(GroupId groupId, long epicIid)
    {
        return EpicRoot(groupId).Segment(epicIid);
    }

    private static GitLabRouteBuilder ChildEpicsRoute(GroupId groupId, long epicIid)
    {
        return EpicRoute(groupId, epicIid).Literal("epics");
    }

    private static GitLabRouteBuilder ChildEpicRoute(GroupId groupId, long epicIid, long childEpicId)
    {
        return ChildEpicsRoute(groupId, epicIid).Segment(childEpicId);
    }

    private static GitLabRouteBuilder EpicIssuesRoute(GroupId groupId, long epicIid)
    {
        return EpicRoute(groupId, epicIid).Literal("issues");
    }

    private static GitLabRouteBuilder EpicBoardsRoute(GroupId groupId)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("epic_boards");
    }

    private static GitLabRouteBuilder EpicBoardListsRoute(GroupId groupId, long boardId)
    {
        return EpicBoardsRoute(groupId).Segment(boardId).Literal("lists");
    }
}