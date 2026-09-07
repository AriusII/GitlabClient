using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class RunnersRepository(IGitLabApiConnection connection) : IRunnersRepository
{
    public IAsyncEnumerable<GitLabRunner> ListAsync(RunnerListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("runners")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabRunner> ListAllAsync(RunnerListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("runners")
                .Literal("all")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabRunner> ListProjectRunnersAsync(ProjectId projectId,
        RunnerListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("runners")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabRunner> ListGroupRunnersAsync(GroupId groupId, RunnerListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("runners")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerArray,
            cancellationToken);
    }

    public Task<GitLabRunner> GetAsync(long runnerId, bool? includeProjects = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("runners")
                .Segment(runnerId)
                .Query("include_projects", includeProjects)
                .Build(),
            GitLabJsonContext.Default.GitLabRunner,
            cancellationToken);
    }

    public Task<GitLabRunner> UpdateAsync(long runnerId, UpdateRunnerRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("runners")
                .Segment(runnerId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateRunnerRequest,
            GitLabJsonContext.Default.GitLabRunner,
            cancellationToken);
    }

    /// <summary>
    ///     GitLab answers this with <c>200</c> and the deleted runner rather than <c>204</c>. The body is of
    ///     no use to a caller that just asked for the runner to be gone, so it is discarded.
    /// </summary>
    public Task DeleteAsync(long runnerId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("runners")
                .Segment(runnerId)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabRunner> AssignToProjectAsync(ProjectId projectId, AssignRunnerRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("runners")
                .Build(),
            request,
            GitLabJsonContext.Default.AssignRunnerRequest,
            GitLabJsonContext.Default.GitLabRunner,
            cancellationToken);
    }

    /// <summary>Also a <c>200</c>-with-body delete; see <see cref="DeleteAsync" />.</summary>
    public Task UnassignFromProjectAsync(ProjectId projectId, long runnerId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("runners")
                .Segment(runnerId)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabJob> ListJobsAsync(long runnerId, RunnerJobListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("runners")
                .Segment(runnerId)
                .Literal("jobs")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabJobArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabRunnerManager> ListManagersAsync(long runnerId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("runners")
                .Segment(runnerId)
                .Literal("managers")
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerManagerArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProject> ListProjectsAsync(long runnerId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("runners")
                .Segment(runnerId)
                .Literal("projects")
                .Build(),
            GitLabJsonContext.Default.GitLabProjectArray,
            cancellationToken);
    }

    public Task<GitLabRunnerToken> ResetRegistrationTokenAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("runners")
                .Literal("reset_registration_token")
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerToken,
            cancellationToken);
    }

    public Task<GitLabRunnerToken> ResetProjectRegistrationTokenAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("runners")
                .Literal("reset_registration_token")
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerToken,
            cancellationToken);
    }

    public Task<GitLabRunnerToken> ResetGroupRegistrationTokenAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("runners")
                .Literal("reset_registration_token")
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerToken,
            cancellationToken);
    }

    public Task<GitLabRunnerToken> ResetAuthenticationTokenAsync(long runnerId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("runners")
                .Segment(runnerId)
                .Literal("reset_authentication_token")
                .Build(),
            GitLabJsonContext.Default.GitLabRunnerToken,
            cancellationToken);
    }
}