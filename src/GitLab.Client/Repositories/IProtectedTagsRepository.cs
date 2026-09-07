using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the ProtectedTags resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IProtectedTagsService), typeof(IProtectedTagsClient))]
internal interface IProtectedTagsRepository
{
    IAsyncEnumerable<GitLabProtectedTag> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedTag> GetAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedTag> ProtectAsync(ProjectId projectId, ProtectTagRequest request,
        CancellationToken cancellationToken = default);

    Task UnprotectAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);
}