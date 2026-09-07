using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Boards, sitting between the public <c>IBoardsClient</c>
///     controller and <c>IBoardsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IBoardsService
{
    IAsyncEnumerable<GitLabBoard> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabBoard> GetForProjectAsync(ProjectId projectId, long boardId,
        CancellationToken cancellationToken = default);

    Task<GitLabBoard> CreateForProjectAsync(ProjectId projectId, CreateBoardRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabBoard> UpdateForProjectAsync(ProjectId projectId, long boardId, UpdateBoardRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, long boardId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBoardList> ListListsForProjectAsync(ProjectId projectId, long boardId,
        CancellationToken cancellationToken = default);

    Task<GitLabBoardList> GetListForProjectAsync(ProjectId projectId, long boardId, long listId,
        CancellationToken cancellationToken = default);

    Task<GitLabBoardList> CreateListForProjectAsync(ProjectId projectId, long boardId,
        CreateBoardListRequest request, CancellationToken cancellationToken = default);

    Task<GitLabBoardList> UpdateListPositionForProjectAsync(ProjectId projectId, long boardId, long listId,
        UpdateBoardListRequest request, CancellationToken cancellationToken = default);

    Task DeleteListForProjectAsync(ProjectId projectId, long boardId, long listId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBoard> ListForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabBoard> GetForGroupAsync(GroupId groupId, long boardId, CancellationToken cancellationToken = default);

    Task<GitLabBoard> CreateForGroupAsync(GroupId groupId, CreateBoardRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabBoard> UpdateForGroupAsync(GroupId groupId, long boardId, UpdateBoardRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, long boardId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBoardList> ListListsForGroupAsync(GroupId groupId, long boardId,
        CancellationToken cancellationToken = default);

    Task<GitLabBoardList> GetListForGroupAsync(GroupId groupId, long boardId, long listId,
        CancellationToken cancellationToken = default);

    Task<GitLabBoardList> CreateListForGroupAsync(GroupId groupId, long boardId, CreateBoardListRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabBoardList> UpdateListPositionForGroupAsync(GroupId groupId, long boardId, long listId,
        UpdateBoardListRequest request, CancellationToken cancellationToken = default);

    Task DeleteListForGroupAsync(GroupId groupId, long boardId, long listId,
        CancellationToken cancellationToken = default);
}