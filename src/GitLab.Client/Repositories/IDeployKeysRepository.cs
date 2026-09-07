using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the DeployKeys resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IDeployKeysService), typeof(IDeployKeysClient))]
internal interface IDeployKeysRepository
{
    IAsyncEnumerable<GitLabDeployKey> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabDeployKey> GetAsync(ProjectId projectId, long keyId, CancellationToken cancellationToken = default);

    Task<GitLabDeployKey> AddAsync(ProjectId projectId, CreateDeployKeyRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabDeployKey> UpdateAsync(ProjectId projectId, long keyId, UpdateDeployKeyRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long keyId, CancellationToken cancellationToken = default);

    Task<GitLabDeployKey> EnableAsync(ProjectId projectId, long keyId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDeployKey> ListAllAsync(bool? publicOnly = null,
        CancellationToken cancellationToken = default);

    Task<GitLabDeployKey> CreateAsync(CreateDeployKeyRequest request, CancellationToken cancellationToken = default);
}