using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for GPG keys: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IGpgKeysService), typeof(IGpgKeysClient))]
internal interface IGpgKeysRepository
{
    IAsyncEnumerable<GitLabGpgKey> ListForCurrentUserAsync(CancellationToken cancellationToken = default);

    Task<GitLabGpgKey> GetForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default);

    Task<GitLabGpgKey> CreateForCurrentUserAsync(CreateGpgKeyRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default);

    Task RevokeForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGpgKey> ListForUserAsync(long userId, CancellationToken cancellationToken = default);

    Task<GitLabGpgKey> GetForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default);

    Task<GitLabGpgKey> CreateForUserAsync(long userId, CreateGpgKeyRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default);

    Task RevokeForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default);
}