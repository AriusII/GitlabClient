using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class DeployKeysClient(IGitLabApiConnection connection) : IDeployKeysClient
{
    public IAsyncEnumerable<GitLabDeployKey> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("deploy_keys").Build(),
            GitLabJsonContext.Default.GitLabDeployKeyArray,
            cancellationToken);
    }

    public Task<GitLabDeployKey> GetAsync(ProjectId projectId, long keyId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("deploy_keys").Segment(keyId).Build(),
            GitLabJsonContext.Default.GitLabDeployKey,
            cancellationToken);
    }

    public Task<GitLabDeployKey> AddAsync(ProjectId projectId, CreateDeployKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("deploy_keys").Build(),
            request,
            GitLabJsonContext.Default.CreateDeployKeyRequest,
            GitLabJsonContext.Default.GitLabDeployKey,
            cancellationToken);
    }

    public Task<GitLabDeployKey> UpdateAsync(ProjectId projectId, long keyId, UpdateDeployKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("deploy_keys").Segment(keyId).Build(),
            request,
            GitLabJsonContext.Default.UpdateDeployKeyRequest,
            GitLabJsonContext.Default.GitLabDeployKey,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long keyId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("deploy_keys").Segment(keyId).Build(),
            cancellationToken);
    }

    public Task<GitLabDeployKey> EnableAsync(ProjectId projectId, long keyId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("deploy_keys").Segment(keyId)
                .Literal("enable").Build(),
            GitLabJsonContext.Default.GitLabDeployKey,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDeployKey> ListAllAsync(bool? publicOnly = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            // GitLab names this filter "public", which is a C# keyword, so the parameter is publicOnly and the
            // wire name is spelled out here rather than derived from it.
            GitLabRouteBuilder.Create("deploy_keys").Query("public", publicOnly).Build(),
            GitLabJsonContext.Default.GitLabDeployKeyArray,
            cancellationToken);
    }

    public Task<GitLabDeployKey> CreateAsync(CreateDeployKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return CreateForInstanceAsync(
            new CreateInstanceDeployKeyRequest
            {
                Key = request.Key, Title = request.Title, ExpiresAt = request.ExpiresAt
            }, cancellationToken);
    }

    public Task<GitLabDeployKey> CreateForInstanceAsync(CreateInstanceDeployKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return connection.PostAsync(
            GitLabRouteBuilder.Create("deploy_keys").Build(),
            request,
            GitLabJsonContext.Default.CreateInstanceDeployKeyRequest,
            GitLabJsonContext.Default.GitLabDeployKey,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDeployKey> ListForUserAsync(long userId,
        UserProjectDeployKeyListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users").Segment(userId).Literal("project_deploy_keys").QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabDeployKeyArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDeployKey> ListForUserAsync(string userId,
        UserProjectDeployKeyListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users").Escaped(userId).Literal("project_deploy_keys").QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabDeployKeyArray,
            cancellationToken);
    }
}