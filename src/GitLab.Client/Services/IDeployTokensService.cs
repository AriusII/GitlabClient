using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Deploy tokens, sitting between the public
///     <c>IDeployTokensClient</c> controller and <c>IDeployTokensRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface IDeployTokensService
{
    IAsyncEnumerable<GitLabDeployToken> ListAsync(DeployTokenListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDeployToken> ListForProjectAsync(ProjectId projectId,
        DeployTokenListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabDeployToken> GetForProjectAsync(ProjectId projectId, long tokenId,
        CancellationToken cancellationToken = default);

    Task<GitLabDeployTokenWithSecret> CreateForProjectAsync(ProjectId projectId, CreateDeployTokenRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, long tokenId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDeployToken> ListForGroupAsync(GroupId groupId, DeployTokenListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabDeployToken> GetForGroupAsync(GroupId groupId, long tokenId,
        CancellationToken cancellationToken = default);

    Task<GitLabDeployTokenWithSecret> CreateForGroupAsync(GroupId groupId, CreateDeployTokenRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, long tokenId, CancellationToken cancellationToken = default);
}