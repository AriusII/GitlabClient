using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Boards resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IBoardsService), typeof(IBoardsClient))]
internal interface IBoardsRepository
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