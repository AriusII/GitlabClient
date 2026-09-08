using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class JobTokenScopeRepository(IGitLabApiConnection connection) : IJobTokenScopeRepository
{
    public Task<GitLabProjectJobTokenScope> GetAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("job_token_scope").Build(),
            GitLabJsonContext.Default.GitLabProjectJobTokenScope,
            cancellationToken);
    }

    public Task UpdateAsync(ProjectId projectId, UpdateProjectJobTokenScopeRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("job_token_scope").Build(),
            request,
            GitLabJsonContext.Default.UpdateProjectJobTokenScopeRequest,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProject> ListAllowlistAsync(ProjectId projectId,
        JobTokenScopeAllowlistListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("job_token_scope")
                .Literal("allowlist").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabProjectArray,
            cancellationToken);
    }

    public Task<GitLabProject> AddToAllowlistAsync(ProjectId projectId, AddProjectToJobTokenAllowlistRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("job_token_scope")
                .Literal("allowlist").Build(),
            request,
            GitLabJsonContext.Default.AddProjectToJobTokenAllowlistRequest,
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public Task RemoveFromAllowlistAsync(ProjectId projectId, long targetProjectId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("job_token_scope")
                .Literal("allowlist").Segment(targetProjectId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabJobTokenScopeGroup> ListGroupsAllowlistAsync(ProjectId projectId,
        JobTokenScopeAllowlistListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("job_token_scope")
                .Literal("groups_allowlist").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabJobTokenScopeGroupArray,
            cancellationToken);
    }

    public Task<GitLabJobTokenScopeGroup> AddGroupToAllowlistAsync(ProjectId projectId,
        AddGroupToJobTokenAllowlistRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("job_token_scope")
                .Literal("groups_allowlist").Build(),
            request,
            GitLabJsonContext.Default.AddGroupToJobTokenAllowlistRequest,
            GitLabJsonContext.Default.GitLabJobTokenScopeGroup,
            cancellationToken);
    }

    public Task RemoveGroupFromAllowlistAsync(ProjectId projectId, long targetGroupId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("job_token_scope")
                .Literal("groups_allowlist").Segment(targetGroupId).Build(),
            cancellationToken);
    }
}