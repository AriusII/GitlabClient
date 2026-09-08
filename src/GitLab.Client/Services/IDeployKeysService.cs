using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for DeployKeys, sitting between the public <c>IDeployKeysClient</c>
///     controller and <c>IDeployKeysRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IDeployKeysService
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

    IAsyncEnumerable<GitLabDeployKey> ListForUserAsync(long userId,
        UserProjectDeployKeyListOptions? options = null, CancellationToken cancellationToken = default);
}