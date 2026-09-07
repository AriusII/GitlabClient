using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for resource-scoped access tokens: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IAccessTokensService), typeof(IAccessTokensClient))]
internal interface IAccessTokensRepository
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