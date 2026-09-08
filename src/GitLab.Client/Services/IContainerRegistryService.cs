using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Container registry, sitting between the public
///     <c>IContainerRegistryClient</c> controller and <c>IContainerRegistryRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this
///     is the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface IContainerRegistryService
{
    Task IngestEventsAsync(JsonElement payload, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabRegistryRepository> ListForGroupAsync(GroupId groupId,
        GroupRegistryRepositoryListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabRegistryRepository> ListForProjectAsync(ProjectId projectId,
        RegistryRepositoryListOptions? options = null, CancellationToken cancellationToken = default);

    Task DeleteRepositoryAsync(ProjectId projectId, long repositoryId,
        CancellationToken cancellationToken = default);

    Task DeleteTagsAsync(ProjectId projectId, long repositoryId, DeleteRegistryRepositoryTagsOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabRegistryRepositoryTag> ListTagsAsync(ProjectId projectId, long repositoryId,
        RegistryRepositoryTagListOptions? options = null, CancellationToken cancellationToken = default);

    Task DeleteTagAsync(ProjectId projectId, long repositoryId, string tagName,
        CancellationToken cancellationToken = default);

    Task<GitLabRegistryRepositoryTagDetails> GetTagAsync(ProjectId projectId, long repositoryId, string tagName,
        CancellationToken cancellationToken = default);

    Task<GitLabRegistryRepository> GetAsync(long repositoryId, RegistryRepositoryGetOptions? options = null,
        CancellationToken cancellationToken = default);
}