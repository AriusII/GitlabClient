using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Tags resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ITagsService), typeof(ITagsClient))]
internal interface ITagsRepository
{
    IAsyncEnumerable<GitLabTag> ListAsync(ProjectId projectId, TagListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabTag> GetAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    Task<GitLabTag> CreateAsync(ProjectId projectId, CreateTagRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    Task<GitLabTagSignature> GetSignatureAsync(ProjectId projectId, string tagName,
        CancellationToken cancellationToken = default);
}