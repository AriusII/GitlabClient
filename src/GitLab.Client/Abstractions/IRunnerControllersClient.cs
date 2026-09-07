using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Runner controllers" API area (<c>/runner_controllers</c>): registering the
///     instance-level component that evaluates CI jobs, choosing which runners it evaluates for, and
///     managing the tokens it authenticates with.
///     <para>
///         A controller is scoped either to the whole instance or to individual runners, never both -
///         see <see cref="AddInstanceScopeAsync" /> and <see cref="AddRunnerScopeAsync" />.
///     </para>
///     <para>This API area carries GitLab's <c>experiment</c> lifecycle marker and may change.</para>
/// </summary>
public interface IRunnerControllersClient
{
    /// <summary>Lists every runner controller on the instance, streaming every page. Administrators only.</summary>
    IAsyncEnumerable<GitLabRunnerController> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one runner controller, including its
    ///     <see cref="GitLabRunnerController.Connected" /> flag, which no other endpoint reports.
    /// </summary>
    Task<GitLabRunnerController> GetAsync(long runnerControllerId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Registers a runner controller. It starts with no scope and no token; add a scope and rotate a
    ///     token before it can do anything.
    /// </summary>
    Task<GitLabRunnerController> RegisterAsync(RegisterRunnerControllerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a runner controller's description or state. Omitted members keep their current value.</summary>
    Task<GitLabRunnerController> UpdateAsync(long runnerControllerId, UpdateRunnerControllerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a runner controller. Administrators only. GitLab echoes the deleted controller back; the
    ///     response is discarded.
    /// </summary>
    Task DeleteAsync(long runnerControllerId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists a controller's scopes, streaming every page. A
    ///     <see cref="GitLabRunnerControllerScope.RunnerId" /> of <see langword="null" /> is the
    ///     instance-wide scope.
    /// </summary>
    IAsyncEnumerable<GitLabRunnerControllerScope> ListScopesAsync(long runnerControllerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Puts a controller in charge of every runner on the instance. A controller may hold only one
    ///     instance scope, and cannot hold runner scopes at the same time; either collision is answered with
    ///     a <see cref="Exceptions.GitLabConflictException" />.
    /// </summary>
    Task<GitLabRunnerControllerScope> AddInstanceScopeAsync(long runnerControllerId,
        CancellationToken cancellationToken = default);

    /// <summary>Removes the instance-wide scope from a controller.</summary>
    Task RemoveInstanceScopeAsync(long runnerControllerId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Puts a controller in charge of one runner. Rejected with a
    ///     <see cref="Exceptions.GitLabConflictException" /> while the controller holds the instance scope -
    ///     call <see cref="RemoveInstanceScopeAsync" /> first.
    /// </summary>
    Task<GitLabRunnerControllerScope> AddRunnerScopeAsync(long runnerControllerId, long runnerId,
        CancellationToken cancellationToken = default);

    /// <summary>Removes one runner from a controller's scope.</summary>
    Task RemoveRunnerScopeAsync(long runnerControllerId, long runnerId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists a controller's tokens, streaming every page. Metadata only - the secrets are not readable,
    ///     by design.
    /// </summary>
    IAsyncEnumerable<GitLabRunnerControllerToken> ListTokensAsync(long runnerControllerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a token for a controller. GitLab answers with the token's metadata and <em>not</em> its
    ///     secret; call <see cref="RotateTokenAsync" /> to obtain a usable value.
    /// </summary>
    Task<GitLabRunnerControllerToken> CreateTokenAsync(long runnerControllerId,
        CreateRunnerControllerTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>Retrieves one token's metadata. The secret is never returned here.</summary>
    Task<GitLabRunnerControllerToken> GetTokenAsync(long runnerControllerId, long tokenId,
        CancellationToken cancellationToken = default);

    /// <summary>Revokes a token immediately. A controller still using it stops authenticating.</summary>
    Task RevokeTokenAsync(long runnerControllerId, long tokenId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates a token, invalidating the old secret and minting a new one.
    ///     <para>
    ///         This is the only call that returns a usable secret, and it returns it exactly once - see
    ///         <see cref="GitLabRunnerControllerTokenWithSecret.Token" />. Persist it immediately, and keep it
    ///         out of logs.
    ///     </para>
    /// </summary>
    Task<GitLabRunnerControllerTokenWithSecret> RotateTokenAsync(long runnerControllerId, long tokenId,
        CancellationToken cancellationToken = default);
}