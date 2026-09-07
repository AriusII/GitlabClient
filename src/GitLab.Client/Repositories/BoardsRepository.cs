using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class BoardsRepository(IGitLabApiConnection connection) : IBoardsRepository
{
    public IAsyncEnumerable<GitLabBoard> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectBoards(projectId).Build(),
            GitLabJsonContext.Default.GitLabBoardArray,
            cancellationToken);
    }

    public Task<GitLabBoard> GetForProjectAsync(ProjectId projectId, long boardId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectBoards(projectId).Segment(boardId).Build(),
            GitLabJsonContext.Default.GitLabBoard,
            cancellationToken);
    }

    public Task<GitLabBoard> CreateForProjectAsync(ProjectId projectId, CreateBoardRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectBoards(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreateBoardRequest,
            GitLabJsonContext.Default.GitLabBoard,
            cancellationToken);
    }

    public Task<GitLabBoard> UpdateForProjectAsync(ProjectId projectId, long boardId, UpdateBoardRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectBoards(projectId).Segment(boardId).Build(),
            request,
            GitLabJsonContext.Default.UpdateBoardRequest,
            GitLabJsonContext.Default.GitLabBoard,
            cancellationToken);
    }

    // The spec declares DELETE as 200-with-the-deleted-entity, but GitLab answers 204 No Content.
    // DeleteAsync only ensures success and ignores the body, so it is correct under either behaviour.
    public Task DeleteForProjectAsync(ProjectId projectId, long boardId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ProjectBoards(projectId).Segment(boardId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBoardList> ListListsForProjectAsync(ProjectId projectId, long boardId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectBoardLists(projectId, boardId).Build(),
            GitLabJsonContext.Default.GitLabBoardListArray,
            cancellationToken);
    }

    public Task<GitLabBoardList> GetListForProjectAsync(ProjectId projectId, long boardId, long listId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectBoardLists(projectId, boardId).Segment(listId).Build(),
            GitLabJsonContext.Default.GitLabBoardList,
            cancellationToken);
    }

    public Task<GitLabBoardList> CreateListForProjectAsync(ProjectId projectId, long boardId,
        CreateBoardListRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectBoardLists(projectId, boardId).Build(),
            request,
            GitLabJsonContext.Default.CreateBoardListRequest,
            GitLabJsonContext.Default.GitLabBoardList,
            cancellationToken);
    }

    public Task<GitLabBoardList> UpdateListPositionForProjectAsync(ProjectId projectId, long boardId, long listId,
        UpdateBoardListRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectBoardLists(projectId, boardId).Segment(listId).Build(),
            request,
            GitLabJsonContext.Default.UpdateBoardListRequest,
            GitLabJsonContext.Default.GitLabBoardList,
            cancellationToken);
    }

    // Same 200-vs-204 mismatch as DeleteForProjectAsync above; the body is ignored either way.
    public Task DeleteListForProjectAsync(ProjectId projectId, long boardId, long listId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ProjectBoardLists(projectId, boardId).Segment(listId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBoard> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GroupBoards(groupId).Build(),
            GitLabJsonContext.Default.GitLabBoardArray,
            cancellationToken);
    }

    public Task<GitLabBoard> GetForGroupAsync(GroupId groupId, long boardId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupBoards(groupId).Segment(boardId).Build(),
            GitLabJsonContext.Default.GitLabBoard,
            cancellationToken);
    }

    public Task<GitLabBoard> CreateForGroupAsync(GroupId groupId, CreateBoardRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GroupBoards(groupId).Build(),
            request,
            GitLabJsonContext.Default.CreateBoardRequest,
            GitLabJsonContext.Default.GitLabBoard,
            cancellationToken);
    }

    public Task<GitLabBoard> UpdateForGroupAsync(GroupId groupId, long boardId, UpdateBoardRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GroupBoards(groupId).Segment(boardId).Build(),
            request,
            GitLabJsonContext.Default.UpdateBoardRequest,
            GitLabJsonContext.Default.GitLabBoard,
            cancellationToken);
    }

    // Same 200-vs-204 mismatch as the project scope; the body is ignored either way.
    public Task DeleteForGroupAsync(GroupId groupId, long boardId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GroupBoards(groupId).Segment(boardId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBoardList> ListListsForGroupAsync(GroupId groupId, long boardId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GroupBoardLists(groupId, boardId).Build(),
            GitLabJsonContext.Default.GitLabBoardListArray,
            cancellationToken);
    }

    public Task<GitLabBoardList> GetListForGroupAsync(GroupId groupId, long boardId, long listId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupBoardLists(groupId, boardId).Segment(listId).Build(),
            GitLabJsonContext.Default.GitLabBoardList,
            cancellationToken);
    }

    public Task<GitLabBoardList> CreateListForGroupAsync(GroupId groupId, long boardId,
        CreateBoardListRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GroupBoardLists(groupId, boardId).Build(),
            request,
            GitLabJsonContext.Default.CreateBoardListRequest,
            GitLabJsonContext.Default.GitLabBoardList,
            cancellationToken);
    }

    public Task<GitLabBoardList> UpdateListPositionForGroupAsync(GroupId groupId, long boardId, long listId,
        UpdateBoardListRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GroupBoardLists(groupId, boardId).Segment(listId).Build(),
            request,
            GitLabJsonContext.Default.UpdateBoardListRequest,
            GitLabJsonContext.Default.GitLabBoardList,
            cancellationToken);
    }

    // Same 200-vs-204 mismatch as the project scope; the body is ignored either way.
    public Task DeleteListForGroupAsync(GroupId groupId, long boardId, long listId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GroupBoardLists(groupId, boardId).Segment(listId).Build(),
            cancellationToken);
    }

    private static GitLabRouteBuilder ProjectBoards(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("boards");
    }

    private static GitLabRouteBuilder ProjectBoardLists(ProjectId projectId, long boardId)
    {
        return ProjectBoards(projectId).Segment(boardId).Literal("lists");
    }

    private static GitLabRouteBuilder GroupBoards(GroupId groupId)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("boards");
    }

    private static GitLabRouteBuilder GroupBoardLists(GroupId groupId, long boardId)
    {
        return GroupBoards(groupId).Segment(boardId).Literal("lists");
    }
}