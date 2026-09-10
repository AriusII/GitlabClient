using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class EnvironmentsClient(IGitLabApiConnection connection) : IEnvironmentsClient
{
    public IAsyncEnumerable<GitLabEnvironment> ListAsync(ProjectId projectId,
        EnvironmentListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("environments")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabEnvironmentArray,
            cancellationToken);
    }

    public Task<GitLabEnvironment> GetAsync(ProjectId projectId, long environmentId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("environments").Segment(environmentId)
                .Build(),
            GitLabJsonContext.Default.GitLabEnvironment,
            cancellationToken);
    }

    public Task<GitLabEnvironment> CreateAsync(ProjectId projectId, CreateEnvironmentRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("environments").Build(),
            request,
            GitLabJsonContext.Default.CreateEnvironmentRequest,
            GitLabJsonContext.Default.GitLabEnvironment,
            cancellationToken);
    }

    public Task<GitLabEnvironment> UpdateAsync(ProjectId projectId, long environmentId,
        UpdateEnvironmentRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("environments").Segment(environmentId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateEnvironmentRequest,
            GitLabJsonContext.Default.GitLabEnvironment,
            cancellationToken);
    }

    public Task<GitLabEnvironment> StopAsync(ProjectId projectId, long environmentId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("environments").Segment(environmentId)
                .Literal("stop").Build(),
            GitLabJsonContext.Default.GitLabEnvironment,
            cancellationToken);
    }

    public Task<GitLabEnvironment> StopAsync(ProjectId projectId, long environmentId, bool force,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("environments").Segment(environmentId)
                .Literal("stop").Build(),
            new StopEnvironmentRequest { Force = force },
            GitLabJsonContext.Default.StopEnvironmentRequest,
            GitLabJsonContext.Default.GitLabEnvironment,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long environmentId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("environments").Segment(environmentId)
                .Build(),
            cancellationToken);
    }

    public Task StopStaleAsync(ProjectId projectId, StopStaleEnvironmentsRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("environments").Literal("stop_stale")
                .Build(),
            request,
            GitLabJsonContext.Default.StopStaleEnvironmentsRequest,
            cancellationToken);
    }

    public Task<GitLabReviewAppDeletionResult> DeleteReviewAppsAsync(ProjectId projectId,
        ReviewAppDeletionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("environments").Literal("review_apps")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabReviewAppDeletionResult,
            cancellationToken);
    }
}