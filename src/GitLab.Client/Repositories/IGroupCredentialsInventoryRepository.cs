using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the group credentials inventory: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IGroupCredentialsInventoryService), typeof(IGroupCredentialsInventoryClient))]
internal interface IGroupCredentialsInventoryRepository
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