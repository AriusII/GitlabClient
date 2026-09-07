using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class ResourceGroupsRepository(IGitLabApiConnection connection) : IResourceGroupsRepository
{
    public IAsyncEnumerable<GitLabResourceGroup> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("resource_groups")
                .Build(),
            GitLabJsonContext.Default.GitLabResourceGroupArray,
            cancellationToken);
    }

    public Task<GitLabResourceGroup> GetAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("resource_groups")
                .Escaped(key)
                .Build(),
            GitLabJsonContext.Default.GitLabResourceGroup,
            cancellationToken);
    }

    public Task<GitLabResourceGroup> UpdateAsync(ProjectId projectId, string key,
        UpdateResourceGroupRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("resource_groups")
                .Escaped(key)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateResourceGroupRequest,
            GitLabJsonContext.Default.GitLabResourceGroup,
            cancellationToken);
    }

    public Task<GitLabJob> GetCurrentJobAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("resource_groups")
                .Escaped(key)
                .Literal("current_job")
                .Build(),
            GitLabJsonContext.Default.GitLabJob,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabJob> ListUpcomingJobsAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("resource_groups")
                .Escaped(key)
                .Literal("upcoming_jobs")
                .Build(),
            GitLabJsonContext.Default.GitLabJobArray,
            cancellationToken);
    }
}