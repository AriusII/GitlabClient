using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Deploy tokens resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IDeployTokensService), typeof(IDeployTokensClient))]
internal interface IDeployTokensRepository
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