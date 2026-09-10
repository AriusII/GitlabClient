using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ProtectedEnvironmentsClient(IGitLabApiConnection connection)
    : IProtectedEnvironmentsClient
{
    public IAsyncEnumerable<GitLabProtectedEnvironment> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("protected_environments").Build(),
            GitLabJsonContext.Default.GitLabProtectedEnvironmentArray,
            cancellationToken);
    }

    public Task<GitLabProtectedEnvironment> GetForProjectAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("protected_environments").Escaped(name)
                .Build(),
            GitLabJsonContext.Default.GitLabProtectedEnvironment,
            cancellationToken);
    }

    public Task<GitLabProtectedEnvironment> ProtectForProjectAsync(ProjectId projectId,
        ProtectEnvironmentRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("protected_environments").Build(),
            request,
            GitLabJsonContext.Default.ProtectEnvironmentRequest,
            GitLabJsonContext.Default.GitLabProtectedEnvironment,
            cancellationToken);
    }

    public Task<GitLabProtectedEnvironment> UpdateForProjectAsync(ProjectId projectId, string name,
        UpdateProtectedEnvironmentRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("protected_environments").Escaped(name)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateProtectedEnvironmentRequest,
            GitLabJsonContext.Default.GitLabProtectedEnvironment,
            cancellationToken);
    }

    public Task UnprotectForProjectAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("protected_environments").Escaped(name)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProtectedEnvironment> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("protected_environments").Build(),
            GitLabJsonContext.Default.GitLabProtectedEnvironmentArray,
            cancellationToken);
    }

    public Task<GitLabProtectedEnvironment> GetForGroupAsync(GroupId groupId, string deploymentTier,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("protected_environments")
                .Escaped(deploymentTier).Build(),
            GitLabJsonContext.Default.GitLabProtectedEnvironment,
            cancellationToken);
    }

    public Task<GitLabProtectedEnvironment> ProtectForGroupAsync(GroupId groupId, ProtectEnvironmentRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("protected_environments").Build(),
            request,
            GitLabJsonContext.Default.ProtectEnvironmentRequest,
            GitLabJsonContext.Default.GitLabProtectedEnvironment,
            cancellationToken);
    }

    public Task<GitLabProtectedEnvironment> UpdateForGroupAsync(GroupId groupId, string deploymentTier,
        UpdateProtectedEnvironmentRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("protected_environments")
                .Escaped(deploymentTier).Build(),
            request,
            GitLabJsonContext.Default.UpdateProtectedEnvironmentRequest,
            GitLabJsonContext.Default.GitLabProtectedEnvironment,
            cancellationToken);
    }

    public Task UnprotectForGroupAsync(GroupId groupId, string deploymentTier,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("protected_environments")
                .Escaped(deploymentTier).Build(),
            cancellationToken);
    }
}