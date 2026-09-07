using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for personal access tokens, service-account tokens and
///     impersonation tokens, sitting between the public <c>IPersonalAccessTokensClient</c> controller
///     and <c>IPersonalAccessTokensRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go.
/// </summary>
internal interface IPersonalAccessTokensService
{
    IAsyncEnumerable<GitLabPersonalAccessToken> ListAsync(PersonalAccessTokenListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabPersonalAccessToken> GetAsync(long tokenId, CancellationToken cancellationToken = default);

    Task RevokeAsync(long tokenId, CancellationToken cancellationToken = default);

    Task<GitLabPersonalAccessTokenWithSecret> RotateAsync(long tokenId, RotateAccessTokenRequest? request = null,
        CancellationToken cancellationToken = default);

    Task<GitLabPersonalAccessToken> GetSelfAsync(CancellationToken cancellationToken = default);

    Task RevokeSelfAsync(CancellationToken cancellationToken = default);

    Task<GitLabPersonalAccessTokenWithSecret> RotateSelfAsync(RotateAccessTokenRequest? request = null,
        CancellationToken cancellationToken = default);

    Task<GitLabTokenAssociations> GetSelfAssociationsAsync(TokenAssociationListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabPersonalAccessTokenWithSecret> CreateForCurrentUserAsync(
        CreateCurrentUserPersonalAccessTokenRequest request, CancellationToken cancellationToken = default);

    Task<GitLabPersonalAccessTokenWithSecret> CreateForUserAsync(long userId,
        CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabPersonalAccessToken> ListForProjectServiceAccountAsync(ProjectId projectId, long userId,
        AccessTokenListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabPersonalAccessTokenWithSecret> CreateForProjectServiceAccountAsync(ProjectId projectId, long userId,
        CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken = default);

    Task<GitLabPersonalAccessTokenWithSecret> RotateForProjectServiceAccountAsync(ProjectId projectId, long userId,
        long tokenId, RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default);

    Task RevokeForProjectServiceAccountAsync(ProjectId projectId, long userId, long tokenId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabPersonalAccessToken> ListForGroupServiceAccountAsync(GroupId groupId, long userId,
        AccessTokenListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabPersonalAccessTokenWithSecret> CreateForGroupServiceAccountAsync(GroupId groupId, long userId,
        CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken = default);

    Task<GitLabPersonalAccessTokenWithSecret> RotateForGroupServiceAccountAsync(GroupId groupId, long userId,
        long tokenId, RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default);

    Task RevokeForGroupServiceAccountAsync(GroupId groupId, long userId, long tokenId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabImpersonationToken> ListImpersonationTokensAsync(long userId,
        ImpersonationTokenListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabImpersonationToken> GetImpersonationTokenAsync(long userId, long impersonationTokenId,
        CancellationToken cancellationToken = default);

    Task<GitLabImpersonationTokenWithSecret> CreateImpersonationTokenAsync(long userId,
        CreateImpersonationTokenRequest request, CancellationToken cancellationToken = default);

    Task RevokeImpersonationTokenAsync(long userId, long impersonationTokenId,
        CancellationToken cancellationToken = default);
}