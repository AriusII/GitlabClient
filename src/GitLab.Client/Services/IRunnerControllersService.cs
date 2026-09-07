using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Runner controllers, sitting between the public
///     <c>IRunnerControllersClient</c> controller and <c>IRunnerControllersRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is
///     the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface IRunnerControllersService
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