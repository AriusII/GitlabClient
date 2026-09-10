using System.Runtime.CompilerServices;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class RunnerControllersClient(IGitLabApiConnection connection) : IRunnerControllersClient
{
    public IAsyncEnumerable<GitLabRunnerController> ListAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerControllerArray,
            cancellationToken);
    }

    public Task<GitLabRunnerController> GetAsync(long runnerControllerId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerController,
            cancellationToken);
    }

    public Task<GitLabRunnerController> RegisterAsync(RegisterRunnerControllerRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Build(),
            request,
            GitLabJsonContext.Default.RegisterRunnerControllerRequest,
            GitLabJsonContext.Default.GitLabRunnerController,
            cancellationToken);
    }

    public Task<GitLabRunnerController> UpdateAsync(long runnerControllerId, UpdateRunnerControllerRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateRunnerControllerRequest,
            GitLabJsonContext.Default.GitLabRunnerController,
            cancellationToken);
    }

    /// <summary>
    ///     GitLab answers this with <c>200</c> and the deleted controller rather than <c>204</c>. The body is
    ///     of no use to a caller that just asked for the controller to be gone, so it is discarded - the same
    ///     treatment <see cref="RunnersClient.DeleteAsync" /> gives the runner delete.
    /// </summary>
    public Task DeleteAsync(long runnerControllerId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabRunnerControllerScopes> GetScopesAsync(long runnerControllerId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Literal("scopes")
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerControllerScopes,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabRunnerControllerScope> ListScopesAsync(long runnerControllerId,
        CancellationToken cancellationToken = default)
    {
        return EnumerateScopesAsync(runnerControllerId, cancellationToken);
    }

    public Task<GitLabRunnerControllerScope> AddInstanceScopeAsync(long runnerControllerId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Literal("scopes")
                .Literal("instance")
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerControllerScope,
            cancellationToken);
    }

    public Task RemoveInstanceScopeAsync(long runnerControllerId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Literal("scopes")
                .Literal("instance")
                .Build(),
            cancellationToken);
    }

    public Task<GitLabRunnerControllerScope> AddRunnerScopeAsync(long runnerControllerId, long runnerId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Literal("scopes")
                .Literal("runners")
                .Segment(runnerId)
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerControllerScope,
            cancellationToken);
    }

    public Task RemoveRunnerScopeAsync(long runnerControllerId, long runnerId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Literal("scopes")
                .Literal("runners")
                .Segment(runnerId)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabRunnerControllerToken> ListTokensAsync(long runnerControllerId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Literal("tokens")
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerControllerTokenArray,
            cancellationToken);
    }

    public Task<GitLabRunnerControllerTokenWithSecret> CreateTokenAsync(long runnerControllerId,
        CreateRunnerControllerTokenRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Literal("tokens")
                .Build(),
            request,
            GitLabJsonContext.Default.CreateRunnerControllerTokenRequest,
            GitLabJsonContext.Default.GitLabRunnerControllerTokenWithSecret,
            cancellationToken);
    }

    public Task<GitLabRunnerControllerToken> GetTokenAsync(long runnerControllerId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Literal("tokens")
                .Segment(tokenId)
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerControllerToken,
            cancellationToken);
    }

    public Task RevokeTokenAsync(long runnerControllerId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Literal("tokens")
                .Segment(tokenId)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabRunnerControllerTokenWithSecret> RotateTokenAsync(long runnerControllerId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("runner_controllers")
                .Segment(runnerControllerId)
                .Literal("tokens")
                .Segment(tokenId)
                .Literal("rotate")
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerControllerTokenWithSecret,
            cancellationToken);
    }

    private async IAsyncEnumerable<GitLabRunnerControllerScope> EnumerateScopesAsync(long runnerControllerId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        GitLabRunnerControllerScopes scopes = await GetScopesAsync(runnerControllerId, cancellationToken)
            .ConfigureAwait(false);

        foreach (GitLabRunnerControllerScope scope in scopes.InstanceLevelScopings ?? [])
        {
            yield return scope;
        }

        foreach (GitLabRunnerControllerScope scope in scopes.RunnerLevelScopings ?? [])
        {
            yield return scope;
        }
    }
}