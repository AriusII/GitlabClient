using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for resource-scoped access tokens, sitting between the public
///     <c>IAccessTokensClient</c> controller and <c>IAccessTokensRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go.
/// </summary>
internal interface IAccessTokensService
{
    IAsyncEnumerable<GitLabAccessToken> ListForProjectAsync(ProjectId projectId,
        AccessTokenListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabAccessToken> GetForProjectAsync(ProjectId projectId, long tokenId,
        CancellationToken cancellationToken = default);

    Task<GitLabAccessTokenWithSecret> CreateForProjectAsync(ProjectId projectId, CreateAccessTokenRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabAccessTokenWithSecret> RotateForProjectAsync(ProjectId projectId, long tokenId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default);

    Task<GitLabAccessTokenWithSecret> RotateSelfForProjectAsync(ProjectId projectId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default);

    Task RevokeForProjectAsync(ProjectId projectId, long tokenId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAccessToken> ListForGroupAsync(GroupId groupId, AccessTokenListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabAccessToken> GetForGroupAsync(GroupId groupId, long tokenId,
        CancellationToken cancellationToken = default);

    Task<GitLabAccessTokenWithSecret> CreateForGroupAsync(GroupId groupId, CreateAccessTokenRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabAccessTokenWithSecret> RotateForGroupAsync(GroupId groupId, long tokenId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default);

    Task<GitLabAccessTokenWithSecret> RotateSelfForGroupAsync(GroupId groupId,
        RotateAccessTokenRequest? request = null, CancellationToken cancellationToken = default);

    Task RevokeForGroupAsync(GroupId groupId, long tokenId, CancellationToken cancellationToken = default);
}