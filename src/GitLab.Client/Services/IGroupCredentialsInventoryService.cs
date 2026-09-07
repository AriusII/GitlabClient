using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the group credentials inventory, sitting between the public
///     <c>IGroupCredentialsInventoryClient</c> controller and
///     <c>IGroupCredentialsInventoryRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IGroupCredentialsInventoryService
{
    IAsyncEnumerable<GitLabPersonalAccessToken> ListPersonalAccessTokensAsync(GroupId groupId,
        PersonalAccessTokenListOptions? options = null, CancellationToken cancellationToken = default);

    Task RevokePersonalAccessTokenAsync(GroupId groupId, long tokenId, CancellationToken cancellationToken = default);

    Task<GitLabPersonalAccessTokenWithSecret> RotatePersonalAccessTokenAsync(GroupId groupId, long tokenId,
        RotateAccessTokenRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAccessToken> ListResourceAccessTokensAsync(GroupId groupId,
        AccessTokenListOptions? options = null, CancellationToken cancellationToken = default);

    Task RevokeResourceAccessTokenAsync(GroupId groupId, long tokenId, DateOnly? expiresAt = null,
        CancellationToken cancellationToken = default);

    Task RotateResourceAccessTokenAsync(GroupId groupId, long tokenId, RotateAccessTokenRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroupManagedSshKey> ListSshKeysAsync(GroupId groupId,
        GroupManagedSshKeyListOptions? options = null, CancellationToken cancellationToken = default);

    Task DeleteSshKeyAsync(GroupId groupId, long keyId, CancellationToken cancellationToken = default);
}