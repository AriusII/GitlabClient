using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Boards" API area (<c>/projects/:id/boards</c> and <c>/groups/:id/boards</c>) -
///     issue boards and the lists (columns) they are made of, at both project and group scope.
/// </summary>
/// <remarks>
///     The two scopes are separate methods rather than overloads on purpose: <c>ProjectId</c> and
///     <c>GroupId</c> both convert implicitly from <c>long</c> and <c>string</c>, so an overload pair would
///     make <c>ListAsync(42)</c> ambiguous at the call site.
/// </remarks>
public interface IBoardsClient
{
    /// <summary>Streams the project's issue boards (<c>GET /projects/:id/boards</c>).</summary>
    IAsyncEnumerable<GitLabBoard> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single project issue board, including its lists.</summary>
    Task<GitLabBoard> GetForProjectAsync(ProjectId projectId, long boardId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a project issue board.</summary>
    Task<GitLabBoard> CreateForProjectAsync(ProjectId projectId, CreateBoardRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a project issue board. Fields left unset on the request are not sent and stay unchanged.</summary>
    Task<GitLabBoard> UpdateForProjectAsync(ProjectId projectId, long boardId, UpdateBoardRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a project issue board.</summary>
    Task DeleteForProjectAsync(ProjectId projectId, long boardId, CancellationToken cancellationToken = default);

    /// <summary>Streams a project board's lists, in board order.</summary>
    IAsyncEnumerable<GitLabBoardList> ListListsForProjectAsync(ProjectId projectId, long boardId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single project board list.</summary>
    Task<GitLabBoardList> GetListForProjectAsync(ProjectId projectId, long boardId, long listId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a project board list. Exactly one of the request's four scope ids must be set - see
    ///     <see cref="CreateBoardListRequest" />.
    /// </summary>
    Task<GitLabBoardList> CreateListForProjectAsync(ProjectId projectId, long boardId,
        CreateBoardListRequest request, CancellationToken cancellationToken = default);

    /// <summary>Moves a project board list to a new position. Position is the only field this endpoint accepts.</summary>
    Task<GitLabBoardList> UpdateListPositionForProjectAsync(ProjectId projectId, long boardId, long listId,
        UpdateBoardListRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a project board list.</summary>
    Task DeleteListForProjectAsync(ProjectId projectId, long boardId, long listId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the group's issue boards (<c>GET /groups/:id/boards</c>).</summary>
    IAsyncEnumerable<GitLabBoard> ListForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single group issue board, including its lists.</summary>
    Task<GitLabBoard> GetForGroupAsync(GroupId groupId, long boardId, CancellationToken cancellationToken = default);

    /// <summary>Creates a group issue board.</summary>
    Task<GitLabBoard> CreateForGroupAsync(GroupId groupId, CreateBoardRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a group issue board. Fields left unset on the request are not sent and stay unchanged.</summary>
    Task<GitLabBoard> UpdateForGroupAsync(GroupId groupId, long boardId, UpdateBoardRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a group issue board.</summary>
    Task DeleteForGroupAsync(GroupId groupId, long boardId, CancellationToken cancellationToken = default);

    /// <summary>Streams a group board's lists, in board order.</summary>
    IAsyncEnumerable<GitLabBoardList> ListListsForGroupAsync(GroupId groupId, long boardId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single group board list.</summary>
    Task<GitLabBoardList> GetListForGroupAsync(GroupId groupId, long boardId, long listId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a group board list. Exactly one of the request's four scope ids must be set - see
    ///     <see cref="CreateBoardListRequest" />.
    /// </summary>
    Task<GitLabBoardList> CreateListForGroupAsync(GroupId groupId, long boardId, CreateBoardListRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Moves a group board list to a new position. Position is the only field this endpoint accepts.</summary>
    Task<GitLabBoardList> UpdateListPositionForGroupAsync(GroupId groupId, long boardId, long listId,
        UpdateBoardListRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a group board list.</summary>
    Task DeleteListForGroupAsync(GroupId groupId, long boardId, long listId,
        CancellationToken cancellationToken = default);
}