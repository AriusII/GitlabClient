using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class StorageMovesClient(IGitLabApiConnection connection) : IStorageMovesClient
{
    // The tag is one route shape instantiated six times, so the words that vary are named once here
    // rather than spelled out at eighteen call sites. Getting "repository_storage_moves" or one of the
    // singular instance-wide roots wrong is a 404 that reads like a missing feature.
    private const string ProjectMovesRoot = "project_repository_storage_moves";
    private const string GroupMovesRoot = "group_repository_storage_moves";
    private const string SnippetMovesRoot = "snippet_repository_storage_moves";
    private const string MovesSegment = "repository_storage_moves";

    public IAsyncEnumerable<GitLabProjectRepositoryStorageMove> ListAllProjectMovesAsync(
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            InstanceFeed(ProjectMovesRoot, options),
            GitLabJsonContext.Default.GitLabProjectRepositoryStorageMoveArray,
            cancellationToken);
    }

    public Task<GitLabProjectRepositoryStorageMove> GetProjectMoveAsync(long repositoryStorageMoveId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            InstanceMove(ProjectMovesRoot, repositoryStorageMoveId),
            GitLabJsonContext.Default.GitLabProjectRepositoryStorageMove,
            cancellationToken);
    }

    public Task ScheduleAllProjectMovesAsync(ScheduleStorageShardMovesRequest request,
        CancellationToken cancellationToken = default)
    {
        // 202 Accepted with an empty body - the bodiless POST overload, not the deserializing one.
        return connection.PostAsync(
            GitLabRouteBuilder.Create(ProjectMovesRoot).Build(),
            request,
            GitLabJsonContext.Default.ScheduleStorageShardMovesRequest,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProjectRepositoryStorageMove> ListForProjectAsync(ProjectId projectId,
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectMoves(projectId).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabProjectRepositoryStorageMoveArray,
            cancellationToken);
    }

    public Task<GitLabProjectRepositoryStorageMove> GetForProjectAsync(ProjectId projectId,
        long repositoryStorageMoveId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectMoves(projectId).Segment(repositoryStorageMoveId).Build(),
            GitLabJsonContext.Default.GitLabProjectRepositoryStorageMove,
            cancellationToken);
    }

    public Task<GitLabProjectRepositoryStorageMove> CreateForProjectAsync(ProjectId projectId,
        CreateStorageMoveRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectMoves(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreateStorageMoveRequest,
            GitLabJsonContext.Default.GitLabProjectRepositoryStorageMove,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroupRepositoryStorageMove> ListAllGroupMovesAsync(
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            InstanceFeed(GroupMovesRoot, options),
            GitLabJsonContext.Default.GitLabGroupRepositoryStorageMoveArray,
            cancellationToken);
    }

    public Task<GitLabGroupRepositoryStorageMove> GetGroupMoveAsync(long repositoryStorageMoveId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            InstanceMove(GroupMovesRoot, repositoryStorageMoveId),
            GitLabJsonContext.Default.GitLabGroupRepositoryStorageMove,
            cancellationToken);
    }

    public Task ScheduleAllGroupMovesAsync(ScheduleStorageShardMovesRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(GroupMovesRoot).Build(),
            request,
            GitLabJsonContext.Default.ScheduleStorageShardMovesRequest,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroupRepositoryStorageMove> ListForGroupAsync(GroupId groupId,
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GroupMoves(groupId).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabGroupRepositoryStorageMoveArray,
            cancellationToken);
    }

    public Task<GitLabGroupRepositoryStorageMove> GetForGroupAsync(GroupId groupId, long repositoryStorageMoveId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupMoves(groupId).Segment(repositoryStorageMoveId).Build(),
            GitLabJsonContext.Default.GitLabGroupRepositoryStorageMove,
            cancellationToken);
    }

    public Task<GitLabGroupRepositoryStorageMove> CreateForGroupAsync(GroupId groupId,
        CreateStorageMoveRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GroupMoves(groupId).Build(),
            request,
            GitLabJsonContext.Default.CreateStorageMoveRequest,
            GitLabJsonContext.Default.GitLabGroupRepositoryStorageMove,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabSnippetRepositoryStorageMove> ListAllSnippetMovesAsync(
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            InstanceFeed(SnippetMovesRoot, options),
            GitLabJsonContext.Default.GitLabSnippetRepositoryStorageMoveArray,
            cancellationToken);
    }

    public Task<GitLabSnippetRepositoryStorageMove> GetSnippetMoveAsync(long repositoryStorageMoveId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            InstanceMove(SnippetMovesRoot, repositoryStorageMoveId),
            GitLabJsonContext.Default.GitLabSnippetRepositoryStorageMove,
            cancellationToken);
    }

    public Task ScheduleAllSnippetMovesAsync(ScheduleStorageShardMovesRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(SnippetMovesRoot).Build(),
            request,
            GitLabJsonContext.Default.ScheduleStorageShardMovesRequest,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabSnippetRepositoryStorageMove> ListForSnippetAsync(long snippetId,
        StorageMoveListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            SnippetMoves(snippetId).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabSnippetRepositoryStorageMoveArray,
            cancellationToken);
    }

    public Task<GitLabSnippetRepositoryStorageMove> GetForSnippetAsync(long snippetId, long repositoryStorageMoveId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            SnippetMoves(snippetId).Segment(repositoryStorageMoveId).Build(),
            GitLabJsonContext.Default.GitLabSnippetRepositoryStorageMove,
            cancellationToken);
    }

    public Task<GitLabSnippetRepositoryStorageMove> CreateForSnippetAsync(long snippetId,
        CreateStorageMoveRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            SnippetMoves(snippetId).Build(),
            request,
            GitLabJsonContext.Default.CreateStorageMoveRequest,
            GitLabJsonContext.Default.GitLabSnippetRepositoryStorageMove,
            cancellationToken);
    }

    // The three instance-wide feeds differ only in their root word, so they share one builder each for
    // the list and the retrieve-by-id shape. The entity-scoped halves cannot collapse the same way -
    // ProjectId, GroupId and long are three different Segment overloads - so each gets a one-line
    // prefix instead, which is still the single place its route is spelled.
    private static Uri InstanceFeed(string root, StorageMoveListOptions? options)
    {
        return GitLabRouteBuilder.Create(root).QueryFrom(options).Build();
    }

    private static Uri InstanceMove(string root, long repositoryStorageMoveId)
    {
        return GitLabRouteBuilder.Create(root).Segment(repositoryStorageMoveId).Build();
    }

    private static GitLabRouteBuilder ProjectMoves(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(MovesSegment);
    }

    private static GitLabRouteBuilder GroupMoves(GroupId groupId)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(MovesSegment);
    }

    // Snippets are addressed by numeric id only - there is no namespaced path form, so no SnippetId
    // value object is warranted the way ProjectId and GroupId are.
    private static GitLabRouteBuilder SnippetMoves(long snippetId)
    {
        return GitLabRouteBuilder.Create("snippets").Segment(snippetId).Literal(MovesSegment);
    }
}