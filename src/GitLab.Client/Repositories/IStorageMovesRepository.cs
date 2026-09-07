using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Storage moves resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         The tag is a three-by-two matrix: projects, groups and snippets each get an instance-wide feed
///         rooted at <c>/&lt;entity&gt;_repository_storage_moves</c> and an entity-scoped one at
///         <c>/&lt;entities&gt;/:id/repository_storage_moves</c>. The two halves are named apart on
///         purpose - <c>...AllProjectMoves...</c> for the instance-wide feed, <c>...ForProject...</c> for
///         a single project - because they take different ids and answer with different scopes.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IStorageMovesService), typeof(IStorageMovesClient))]
internal interface IStorageMovesRepository
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