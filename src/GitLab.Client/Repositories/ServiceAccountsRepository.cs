using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class ServiceAccountsRepository(IGitLabApiConnection connection) : IServiceAccountsRepository
{
    public IAsyncEnumerable<GitLabServiceAccount> ListAsync(ServiceAccountListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("service_accounts").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabServiceAccountArray,
            cancellationToken);
    }

    public Task<GitLabServiceAccount> CreateAsync(CreateServiceAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("service_accounts").Build(),
            request,
            GitLabJsonContext.Default.CreateServiceAccountRequest,
            GitLabJsonContext.Default.GitLabServiceAccount,
            cancellationToken);
    }

    public Task<GitLabServiceAccount> UpdateAsync(long userId, UpdateServiceAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("service_accounts").Segment(userId).Build(),
            request,
            GitLabJsonContext.Default.UpdateServiceAccountRequest,
            GitLabJsonContext.Default.GitLabServiceAccount,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabServiceAccount> ListForGroupAsync(GroupId groupId,
        ServiceAccountListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("service_accounts").QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabServiceAccountArray,
            cancellationToken);
    }

    public Task<GitLabServiceAccount> GetForGroupAsync(GroupId groupId, long userId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("service_accounts").Segment(userId).Build(),
            GitLabJsonContext.Default.GitLabServiceAccount,
            cancellationToken);
    }

    public Task<GitLabServiceAccount> CreateForGroupAsync(GroupId groupId, CreateServiceAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("service_accounts").Build(),
            request,
            GitLabJsonContext.Default.CreateServiceAccountRequest,
            GitLabJsonContext.Default.GitLabServiceAccount,
            cancellationToken);
    }

    public Task<GitLabServiceAccount> UpdateForGroupAsync(GroupId groupId, long userId,
        UpdateServiceAccountRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("service_accounts").Segment(userId).Build(),
            request,
            GitLabJsonContext.Default.UpdateServiceAccountRequest,
            GitLabJsonContext.Default.GitLabServiceAccount,
            cancellationToken);
    }

    public Task DeleteForGroupAsync(GroupId groupId, long userId, bool? hardDelete = null,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("service_accounts").Segment(userId)
                .Query("hard_delete", hardDelete).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabServiceAccount> ListForProjectAsync(ProjectId projectId,
        ServiceAccountListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("service_accounts").QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabServiceAccountArray,
            cancellationToken);
    }

    public Task<GitLabServiceAccount> GetForProjectAsync(ProjectId projectId, long userId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("service_accounts").Segment(userId)
                .Build(),
            GitLabJsonContext.Default.GitLabServiceAccount,
            cancellationToken);
    }

    public Task<GitLabServiceAccount> CreateForProjectAsync(ProjectId projectId, CreateServiceAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("service_accounts").Build(),
            request,
            GitLabJsonContext.Default.CreateServiceAccountRequest,
            GitLabJsonContext.Default.GitLabServiceAccount,
            cancellationToken);
    }

    public Task<GitLabServiceAccount> UpdateForProjectAsync(ProjectId projectId, long userId,
        UpdateServiceAccountRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("service_accounts").Segment(userId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateServiceAccountRequest,
            GitLabJsonContext.Default.GitLabServiceAccount,
            cancellationToken);
    }

    public Task DeleteForProjectAsync(ProjectId projectId, long userId, bool? hardDelete = null,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("service_accounts").Segment(userId)
                .Query("hard_delete", hardDelete).Build(),
            cancellationToken);
    }
}