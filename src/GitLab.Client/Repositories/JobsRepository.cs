using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class JobsRepository(IGitLabApiConnection connection) : IJobsRepository
{
    public IAsyncEnumerable<GitLabJob> ListAsync(ProjectId projectId, JobListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("jobs")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabJobArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabJob> ListForPipelineAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("pipelines").Segment(pipelineId)
                .Literal("jobs").Build(),
            GitLabJsonContext.Default.GitLabJobArray,
            cancellationToken);
    }

    public Task<GitLabJob> GetAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("jobs").Segment(jobId).Build(),
            GitLabJsonContext.Default.GitLabJob,
            cancellationToken);
    }

    public Task<GitLabJob> CancelAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("jobs").Segment(jobId).Literal("cancel")
                .Build(),
            GitLabJsonContext.Default.GitLabJob,
            cancellationToken);
    }

    public Task<GitLabJob> RetryAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("jobs").Segment(jobId).Literal("retry")
                .Build(),
            GitLabJsonContext.Default.GitLabJob,
            cancellationToken);
    }

    public Task<GitLabJob> PlayAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("jobs").Segment(jobId).Literal("play")
                .Build(),
            GitLabJsonContext.Default.GitLabJob,
            cancellationToken);
    }

    public Task<GitLabJob> EraseAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("jobs").Segment(jobId).Literal("erase")
                .Build(),
            GitLabJsonContext.Default.GitLabJob,
            cancellationToken);
    }

    public Task<GitLabJob> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("job").Build(),
            GitLabJsonContext.Default.GitLabJob,
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetTraceAsync(ProjectId projectId, long jobId,
        JobTraceOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("jobs")
                .Segment(jobId)
                .Literal("trace")
                .QueryFrom(options)
                .Build(),
            cancellationToken);
    }
}