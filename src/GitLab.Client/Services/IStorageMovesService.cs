using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Storage moves, sitting between the public
///     <c>IStorageMovesClient</c> controller and <c>IStorageMovesRepository</c> raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IStorageMovesService
{
    IAsyncEnumerable<GitLabProjectRepositoryStorageMove> ListAllProjectMovesAsync(
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabProjectRepositoryStorageMove> GetProjectMoveAsync(long repositoryStorageMoveId,
        CancellationToken cancellationToken = default);

    Task ScheduleAllProjectMovesAsync(ScheduleStorageShardMovesRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProjectRepositoryStorageMove> ListForProjectAsync(ProjectId projectId,
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabProjectRepositoryStorageMove> GetForProjectAsync(ProjectId projectId, long repositoryStorageMoveId,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectRepositoryStorageMove> CreateForProjectAsync(ProjectId projectId,
        CreateStorageMoveRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroupRepositoryStorageMove> ListAllGroupMovesAsync(StorageMoveListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabGroupRepositoryStorageMove> GetGroupMoveAsync(long repositoryStorageMoveId,
        CancellationToken cancellationToken = default);

    Task ScheduleAllGroupMovesAsync(ScheduleStorageShardMovesRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroupRepositoryStorageMove> ListForGroupAsync(GroupId groupId,
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabGroupRepositoryStorageMove> GetForGroupAsync(GroupId groupId, long repositoryStorageMoveId,
        CancellationToken cancellationToken = default);

    Task<GitLabGroupRepositoryStorageMove> CreateForGroupAsync(GroupId groupId, CreateStorageMoveRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabSnippetRepositoryStorageMove> ListAllSnippetMovesAsync(
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabSnippetRepositoryStorageMove> GetSnippetMoveAsync(long repositoryStorageMoveId,
        CancellationToken cancellationToken = default);

    Task ScheduleAllSnippetMovesAsync(ScheduleStorageShardMovesRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabSnippetRepositoryStorageMove> ListForSnippetAsync(long snippetId,
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabSnippetRepositoryStorageMove> GetForSnippetAsync(long snippetId, long repositoryStorageMoveId,
        CancellationToken cancellationToken = default);

    Task<GitLabSnippetRepositoryStorageMove> CreateForSnippetAsync(long snippetId, CreateStorageMoveRequest request,
        CancellationToken cancellationToken = default);
}