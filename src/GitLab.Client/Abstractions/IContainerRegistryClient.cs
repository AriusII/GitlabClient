using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Container registry" API area: the webhook the container registry itself calls
///     back into GitLab on push, delete and other registry operations
///     (<c>/container_registry_event/events</c>), plus the ordinary read/delete surface over registry
///     repositories and their tags (<c>/projects/:id/registry/repositories</c>,
///     <c>/groups/:id/registry/repositories</c>, <c>/registry/repositories/:id</c>). There is no create
///     or update here - repositories and tags come into existence only by pushing an image, never
///     through this API.
/// </summary>
public interface IContainerRegistryClient
{
    /// <summary>
    ///     Receives one notification envelope from the container registry
    ///     (<c>POST /container_registry_event/events</c>). The payload is the registry's own
    ///     <c>application/vnd.docker.distribution.events.v1+json</c> shape, which GitLab's OpenAPI document
    ///     does not type, so it is carried as a raw <see cref="JsonElement" /> rather than an invented DTO.
    ///     Not something an ordinary API consumer calls - it exists so a self-managed registry can be
    ///     pointed at this endpoint.
    /// </summary>
    Task IngestEventsAsync(JsonElement payload, CancellationToken cancellationToken = default);

    /// <summary>Streams every registry repository across a group's projects (<c>GET /groups/:id/registry/repositories</c>).</summary>
    IAsyncEnumerable<GitLabRegistryRepository> ListForGroupAsync(GroupId groupId,
        GroupRegistryRepositoryListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Streams every registry repository on a project (<c>GET /projects/:id/registry/repositories</c>).</summary>
    IAsyncEnumerable<GitLabRegistryRepository> ListForProjectAsync(ProjectId projectId,
        RegistryRepositoryListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a registry repository and every image it holds
    ///     (<c>DELETE /projects/:id/registry/repositories/:repository_id</c>). GitLab performs the actual
    ///     removal asynchronously; the call itself only schedules it.
    /// </summary>
    Task DeleteRepositoryAsync(ProjectId projectId, long repositoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes every tag on a repository matching the given filters
    ///     (<c>DELETE /projects/:id/registry/repositories/:repository_id/tags</c>) - the bulk cleanup
    ///     operation behind GitLab's container registry cleanup policies. GitLab performs the actual
    ///     removal asynchronously; the call itself only schedules it. A deletion matcher
    ///     (<see cref="DeleteRegistryRepositoryTagsOptions.NameRegexDelete" /> or its legacy
    ///     <see cref="DeleteRegistryRepositoryTagsOptions.NameRegex" /> alias) must be supplied; the client
    ///     rejects a null or empty matcher before it issues the potentially destructive request.
    /// </summary>
    Task DeleteTagsAsync(ProjectId projectId, long repositoryId, DeleteRegistryRepositoryTagsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every tag on a repository (<c>GET /projects/:id/registry/repositories/:repository_id/tags</c>).</summary>
    IAsyncEnumerable<GitLabRegistryRepositoryTag> ListTagsAsync(ProjectId projectId, long repositoryId,
        RegistryRepositoryTagListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes one tag by name (<c>DELETE /projects/:id/registry/repositories/:repository_id/tags/:tag_name</c>).</summary>
    Task DeleteTagAsync(ProjectId projectId, long repositoryId, string tagName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one tag's full detail, including its digest and size (
    ///     <c>GET /projects/:id/registry/repositories/:repository_id/tags/:tag_name</c>).
    /// </summary>
    Task<GitLabRegistryRepositoryTagDetails> GetTagAsync(ProjectId projectId, long repositoryId, string tagName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one registry repository by its own ID, independent of the project it belongs to
    ///     (<c>GET /registry/repositories/:id</c>).
    /// </summary>
    Task<GitLabRegistryRepository> GetAsync(long repositoryId, RegistryRepositoryGetOptions? options = null,
        CancellationToken cancellationToken = default);
}