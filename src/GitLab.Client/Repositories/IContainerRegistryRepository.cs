using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Container registry resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IContainerRegistryService), typeof(IContainerRegistryClient))]
internal interface IContainerRegistryRepository
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