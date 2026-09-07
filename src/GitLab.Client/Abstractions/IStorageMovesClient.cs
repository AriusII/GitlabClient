using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Storage moves" API area - scheduling and tracking repository storage moves for
///     projects, groups and snippets while migrating between Gitaly storage shards.
///     <para>
///         <b>Every endpoint here requires instance administrator rights.</b> A non-administrator token
///         gets <c>403 Forbidden</c> (surfaced as
///         <see cref="Exceptions.GitLabForbiddenException" />), so this resource is unusable against
///         GitLab.com and only meaningful on a self-managed instance.
///     </para>
///     <para>
///         The area is a three-by-two matrix. Each of the three entity kinds has an <em>instance-wide</em>
///         feed rooted at <c>/&lt;entity&gt;_repository_storage_moves</c> - list, retrieve one, and
///         schedule a move for everything on a source shard - and an <em>entity-scoped</em> one at
///         <c>/&lt;entities&gt;/:id/repository_storage_moves</c> - list, retrieve one, and create a move
///         for that single entity. The <c>ListAll…</c>/<c>Get…Move</c>/<c>ScheduleAll…</c> methods are the
///         instance-wide half; the <c>…ForProject</c>/<c>…ForGroup</c>/<c>…ForSnippet</c> methods are the
///         entity-scoped half.
///     </para>
///     <para>
///         What actually moves differs by kind: a project move carries the project's repository plus its
///         wiki and design repositories, a group move carries only the <em>group wiki</em> repository (not
///         the repositories of the projects inside the group), and a snippet move carries the snippet
///         repository.
///     </para>
///     <para>
///         Moves are asynchronous. Creating one returns immediately with the move in
///         <see cref="GitLabRepositoryStorageMoveState.Scheduled" />; poll the corresponding
///         <c>Get…</c> method until it reaches <see cref="GitLabRepositoryStorageMoveState.Finished" /> or
///         <see cref="GitLabRepositoryStorageMoveState.Failed" />.
///     </para>
/// </summary>
public interface IStorageMovesClient
{
    /// <summary>
    ///     Streams every project repository storage move on the instance
    ///     (<c>GET /project_repository_storage_moves</c>), newest first.
    /// </summary>
    IAsyncEnumerable<GitLabProjectRepositoryStorageMove> ListAllProjectMovesAsync(
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one project repository storage move by its own id - not by project id
    ///     (<c>GET /project_repository_storage_moves/:repository_storage_move_id</c>).
    /// </summary>
    Task<GitLabProjectRepositoryStorageMove> GetProjectMoveAsync(long repositoryStorageMoveId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Schedules a repository storage move for <em>every</em> project stored on
    ///     <see cref="ScheduleStorageShardMovesRequest.SourceStorageName" />
    ///     (<c>POST /project_repository_storage_moves</c>) - the way a shard is drained.
    ///     <para>
    ///         GitLab answers <c>202 Accepted</c> with no body: the moves are enqueued, not performed. Track
    ///         them with <see cref="ListAllProjectMovesAsync" />.
    ///     </para>
    /// </summary>
    Task ScheduleAllProjectMovesAsync(ScheduleStorageShardMovesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the repository storage moves of one project
    ///     (<c>GET /projects/:id/repository_storage_moves</c>).
    /// </summary>
    IAsyncEnumerable<GitLabProjectRepositoryStorageMove> ListForProjectAsync(ProjectId projectId,
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one of a project's repository storage moves
    ///     (<c>GET /projects/:id/repository_storage_moves/:repository_storage_move_id</c>).
    /// </summary>
    Task<GitLabProjectRepositoryStorageMove> GetForProjectAsync(ProjectId projectId, long repositoryStorageMoveId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Schedules a repository storage move for one project
    ///     (<c>POST /projects/:id/repository_storage_moves</c>). Leave
    ///     <see cref="CreateStorageMoveRequest.DestinationStorageName" /> unset to let GitLab pick a
    ///     destination shard.
    /// </summary>
    Task<GitLabProjectRepositoryStorageMove> CreateForProjectAsync(ProjectId projectId,
        CreateStorageMoveRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every group repository storage move on the instance
    ///     (<c>GET /group_repository_storage_moves</c>).
    /// </summary>
    IAsyncEnumerable<GitLabGroupRepositoryStorageMove> ListAllGroupMovesAsync(StorageMoveListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one group repository storage move by its own id - not by group id
    ///     (<c>GET /group_repository_storage_moves/:repository_storage_move_id</c>).
    /// </summary>
    Task<GitLabGroupRepositoryStorageMove> GetGroupMoveAsync(long repositoryStorageMoveId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Schedules a wiki repository storage move for <em>every</em> group stored on
    ///     <see cref="ScheduleStorageShardMovesRequest.SourceStorageName" />
    ///     (<c>POST /group_repository_storage_moves</c>). Answers <c>202 Accepted</c> with no body.
    /// </summary>
    Task ScheduleAllGroupMovesAsync(ScheduleStorageShardMovesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the repository storage moves of one group
    ///     (<c>GET /groups/:id/repository_storage_moves</c>).
    /// </summary>
    IAsyncEnumerable<GitLabGroupRepositoryStorageMove> ListForGroupAsync(GroupId groupId,
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one of a group's repository storage moves
    ///     (<c>GET /groups/:id/repository_storage_moves/:repository_storage_move_id</c>).
    /// </summary>
    Task<GitLabGroupRepositoryStorageMove> GetForGroupAsync(GroupId groupId, long repositoryStorageMoveId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Schedules a repository storage move for one group
    ///     (<c>POST /groups/:id/repository_storage_moves</c>). This moves the <em>group wiki</em>
    ///     repository only; the projects inside the group need moves of their own.
    /// </summary>
    Task<GitLabGroupRepositoryStorageMove> CreateForGroupAsync(GroupId groupId, CreateStorageMoveRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every snippet repository storage move on the instance
    ///     (<c>GET /snippet_repository_storage_moves</c>).
    /// </summary>
    IAsyncEnumerable<GitLabSnippetRepositoryStorageMove> ListAllSnippetMovesAsync(
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one snippet repository storage move by its own id - not by snippet id
    ///     (<c>GET /snippet_repository_storage_moves/:repository_storage_move_id</c>).
    /// </summary>
    Task<GitLabSnippetRepositoryStorageMove> GetSnippetMoveAsync(long repositoryStorageMoveId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Schedules a repository storage move for <em>every</em> snippet stored on
    ///     <see cref="ScheduleStorageShardMovesRequest.SourceStorageName" />
    ///     (<c>POST /snippet_repository_storage_moves</c>). Answers <c>202 Accepted</c> with no body.
    /// </summary>
    Task ScheduleAllSnippetMovesAsync(ScheduleStorageShardMovesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the repository storage moves of one snippet
    ///     (<c>GET /snippets/:id/repository_storage_moves</c>). Snippets are addressed by numeric id -
    ///     they have no namespaced path form, unlike projects and groups.
    /// </summary>
    IAsyncEnumerable<GitLabSnippetRepositoryStorageMove> ListForSnippetAsync(long snippetId,
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one of a snippet's repository storage moves
    ///     (<c>GET /snippets/:id/repository_storage_moves/:repository_storage_move_id</c>).
    /// </summary>
    Task<GitLabSnippetRepositoryStorageMove> GetForSnippetAsync(long snippetId, long repositoryStorageMoveId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Schedules a repository storage move for one snippet
    ///     (<c>POST /snippets/:id/repository_storage_moves</c>).
    /// </summary>
    Task<GitLabSnippetRepositoryStorageMove> CreateForSnippetAsync(long snippetId, CreateStorageMoveRequest request,
        CancellationToken cancellationToken = default);
}