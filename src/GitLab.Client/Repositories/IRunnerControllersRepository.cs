using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the RunnerControllers resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IRunnerControllersService), typeof(IRunnerControllersClient))]
internal interface IRunnerControllersRepository
{
    IAsyncEnumerable<GitLabRunnerController> ListAsync(CancellationToken cancellationToken = default);

    Task<GitLabRunnerController> GetAsync(long runnerControllerId, CancellationToken cancellationToken = default);

    Task<GitLabRunnerController> RegisterAsync(RegisterRunnerControllerRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabRunnerController> UpdateAsync(long runnerControllerId, UpdateRunnerControllerRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long runnerControllerId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabRunnerControllerScope> ListScopesAsync(long runnerControllerId,
        CancellationToken cancellationToken = default);

    Task<GitLabRunnerControllerScope> AddInstanceScopeAsync(long runnerControllerId,
        CancellationToken cancellationToken = default);

    Task RemoveInstanceScopeAsync(long runnerControllerId, CancellationToken cancellationToken = default);

    Task<GitLabRunnerControllerScope> AddRunnerScopeAsync(long runnerControllerId, long runnerId,
        CancellationToken cancellationToken = default);

    Task RemoveRunnerScopeAsync(long runnerControllerId, long runnerId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabRunnerControllerToken> ListTokensAsync(long runnerControllerId,
        CancellationToken cancellationToken = default);

    Task<GitLabRunnerControllerToken> CreateTokenAsync(long runnerControllerId,
        CreateRunnerControllerTokenRequest request, CancellationToken cancellationToken = default);

    Task<GitLabRunnerControllerToken> GetTokenAsync(long runnerControllerId, long tokenId,
        CancellationToken cancellationToken = default);

    Task RevokeTokenAsync(long runnerControllerId, long tokenId, CancellationToken cancellationToken = default);

    Task<GitLabRunnerControllerTokenWithSecret> RotateTokenAsync(long runnerControllerId, long tokenId,
        CancellationToken cancellationToken = default);
}