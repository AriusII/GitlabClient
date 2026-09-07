using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class DeployTokensRepository(IGitLabApiConnection connection) : IDeployTokensRepository
{
    public IAsyncEnumerable<GitLabDeployToken> ListAsync(DeployTokenListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("deploy_tokens").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabDeployTokenArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDeployToken> ListForProjectAsync(ProjectId projectId,
        DeployTokenListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("deploy_tokens").QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabDeployTokenArray,
            cancellationToken);
    }

    public Task<GitLabDeployToken> GetForProjectAsync(ProjectId projectId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("deploy_tokens").Segment(tokenId).Build(),
            GitLabJsonContext.Default.GitLabDeployToken,
            cancellationToken);
    }

    public Task<GitLabDeployTokenWithSecret> CreateForProjectAsync(ProjectId projectId,
        CreateDeployTokenRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("deploy_tokens").Build(),
            request,
            GitLabJsonContext.Default.CreateDeployTokenRequest,
            GitLabJsonContext.Default.GitLabDeployTokenWithSecret,
            cancellationToken);
    }

    public Task DeleteForProjectAsync(ProjectId projectId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("deploy_tokens").Segment(tokenId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDeployToken> ListForGroupAsync(GroupId groupId,
        DeployTokenListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("deploy_tokens").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabDeployTokenArray,
            cancellationToken);
    }

    public Task<GitLabDeployToken> GetForGroupAsync(GroupId groupId, long tokenId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("deploy_tokens").Segment(tokenId).Build(),
            GitLabJsonContext.Default.GitLabDeployToken,
            cancellationToken);
    }

    public Task<GitLabDeployTokenWithSecret> CreateForGroupAsync(GroupId groupId, CreateDeployTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("deploy_tokens").Build(),
            request,
            GitLabJsonContext.Default.CreateDeployTokenRequest,
            GitLabJsonContext.Default.GitLabDeployTokenWithSecret,
            cancellationToken);
    }

    public Task DeleteForGroupAsync(GroupId groupId, long tokenId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("deploy_tokens").Segment(tokenId).Build(),
            cancellationToken);
    }
}