using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class ContainerRegistryRepository(IGitLabApiConnection connection) : IContainerRegistryRepository
{
    /// <summary>
    ///     The spec declares neither a request nor a response schema for this route - the body is the
    ///     container registry's own notification envelope
    ///     (<c>application/vnd.docker.distribution.events.v1+json</c>), not something GitLab's OpenAPI
    ///     document types - so it is carried as a raw <see cref="JsonElement" /> rather than an invented DTO.
    /// </summary>
    public Task IngestEventsAsync(JsonElement payload, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("container_registry_event")
                .Literal("events")
                .Build(),
            payload,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabRegistryRepository> ListForGroupAsync(GroupId groupId,
        GroupRegistryRepositoryListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("registry")
                .Literal("repositories")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabRegistryRepositoryArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabRegistryRepository> ListForProjectAsync(ProjectId projectId,
        RegistryRepositoryListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectRepositoriesRoute(projectId).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabRegistryRepositoryArray,
            cancellationToken);
    }

    public Task DeleteRepositoryAsync(ProjectId projectId, long repositoryId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ProjectRepositoriesRoute(projectId).Segment(repositoryId).Build(),
            cancellationToken);
    }

    public Task DeleteTagsAsync(ProjectId projectId, long repositoryId,
        DeleteRegistryRepositoryTagsOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ProjectRepositoriesRoute(projectId).Segment(repositoryId).Literal("tags").QueryFrom(options).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabRegistryRepositoryTag> ListTagsAsync(ProjectId projectId, long repositoryId,
        RegistryRepositoryTagListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectRepositoriesRoute(projectId).Segment(repositoryId).Literal("tags").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabRegistryRepositoryTagArray,
            cancellationToken);
    }

    public Task DeleteTagAsync(ProjectId projectId, long repositoryId, string tagName,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ProjectRepositoriesRoute(projectId).Segment(repositoryId).Literal("tags").Escaped(tagName).Build(),
            cancellationToken);
    }

    public Task<GitLabRegistryRepositoryTagDetails> GetTagAsync(ProjectId projectId, long repositoryId,
        string tagName, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectRepositoriesRoute(projectId).Segment(repositoryId).Literal("tags").Escaped(tagName).Build(),
            GitLabJsonContext.Default.GitLabRegistryRepositoryTagDetails,
            cancellationToken);
    }

    public Task<GitLabRegistryRepository> GetAsync(long repositoryId, RegistryRepositoryGetOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("registry")
                .Literal("repositories")
                .Segment(repositoryId)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabRegistryRepository,
            cancellationToken);
    }

    private static GitLabRouteBuilder ProjectRepositoriesRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("registry").Literal("repositories");
    }
}