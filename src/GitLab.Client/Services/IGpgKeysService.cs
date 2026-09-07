using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for GPG keys, sitting between the public <c>IGpgKeysClient</c>
///     controller and <c>IGpgKeysRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go.
/// </summary>
internal interface IGpgKeysService
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