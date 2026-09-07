using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class JobArtifactsRepository(IGitLabApiConnection connection) : IJobArtifactsRepository
{
    public Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, long jobId,
        JobArtifactDownloadOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("jobs")
                .Segment(jobId)
                .Literal("artifacts")
                .QueryFrom(options)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadFileAsync(ProjectId projectId, long jobId, string artifactPath,
        string? jobToken = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("jobs")
                .Segment(jobId)
                .Literal("artifacts")
                .Escaped(artifactPath)
                .Query("job_token", jobToken)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadForRefAsync(ProjectId projectId, string refName, string jobName,
        JobArtifactRefDownloadOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("jobs")
                .Literal("artifacts")
                .Escaped(refName)
                .Literal("download")
                .Query("job", jobName)
                .QueryFrom(options)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadFileForRefAsync(ProjectId projectId, string refName,
        string artifactPath, string jobName, JobArtifactRefDownloadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("jobs")
                .Literal("artifacts")
                .Escaped(refName)
                .Literal("raw")
                .Escaped(artifactPath)
                .Query("job", jobName)
                .QueryFrom(options)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabJobArtifactEntry> ListAsync(ProjectId projectId, long jobId,
        JobArtifactTreeListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("jobs")
                .Segment(jobId)
                .Literal("artifacts")
                .Literal("tree")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabJobArtifactEntryArray,
            cancellationToken);
    }

    public Task<GitLabJob> KeepAsync(ProjectId projectId, long jobId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("jobs")
                .Segment(jobId)
                .Literal("artifacts")
                .Literal("keep")
                .Build(),
            GitLabJsonContext.Default.GitLabJob,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("jobs")
                .Segment(jobId)
                .Literal("artifacts")
                .Build(),
            cancellationToken);
    }

    public Task DeleteAllAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("artifacts")
                .Build(),
            cancellationToken);
    }
}