using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class JobsClient(IGitLabApiConnection connection) : IJobsClient
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

    public Task<GitLabJobRequestResponse> RequestAsync(JobRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("jobs").Literal("request").Build(),
            request,
            GitLabJsonContext.Default.JobRequestRequest,
            GitLabJsonContext.Default.GitLabJobRequestResponse,
            cancellationToken);
    }

    public Task UpdateAsync(long jobId, UpdateJobStateRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            JobRoute(jobId).Build(),
            request,
            GitLabJsonContext.Default.UpdateJobStateRequest,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadArtifactsByTokenAsync(long jobId,
        JobArtifactsByTokenDownloadOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            JobRoute(jobId).Literal("artifacts").QueryFrom(options).Build(),
            cancellationToken);
    }

    public Task UploadArtifactsAsync(long jobId, GitLabFileUpload file, string? token = null,
        string? expireIn = null, GitLabJobArtifactUploadType? artifactType = null,
        GitLabJobArtifactUploadFormat? artifactFormat = null, string? accessibility = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PostFileAsync(
            JobRoute(jobId).Literal("artifacts").Build(),
            file,
            BuildArtifactUploadFormFields(token, expireIn, artifactType, artifactFormat, accessibility),
            cancellationToken);
    }

    public Task AuthorizeArtifactsUploadAsync(long jobId, AuthorizeJobArtifactsUploadRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            JobRoute(jobId).Literal("artifacts").Literal("authorize").Build(),
            request ?? new AuthorizeJobArtifactsUploadRequest(),
            GitLabJsonContext.Default.AuthorizeJobArtifactsUploadRequest,
            cancellationToken);
    }

    public Task AppendTraceAsync(long jobId, AppendJobTraceRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            JobRoute(jobId).Literal("trace").Build(),
            request,
            GitLabJsonContext.Default.AppendJobTraceRequest,
            cancellationToken);
    }

    /// <summary>
    ///     The bare, project-agnostic <c>/jobs/:id</c> route the runner protocol uses - distinct from
    ///     every other route in this class, which is scoped under <c>/projects/:id/jobs</c>.
    /// </summary>
    private static GitLabRouteBuilder JobRoute(long jobId)
    {
        return GitLabRouteBuilder.Create("jobs").Segment(jobId);
    }

    private static Dictionary<string, string>? BuildArtifactUploadFormFields(string? token, string? expireIn,
        GitLabJobArtifactUploadType? artifactType, GitLabJobArtifactUploadFormat? artifactFormat,
        string? accessibility)
    {
        Dictionary<string, string>? formFields = null;

        void AddField(string name, string? value)
        {
            if (value is null)
            {
                return;
            }

            formFields ??= new Dictionary<string, string>(StringComparer.Ordinal);
            formFields[name] = value;
        }

        AddField("token", token);
        AddField("expire_in", expireIn);
        AddField("artifact_type", artifactType.ToApiValue());
        AddField("artifact_format", artifactFormat.ToApiValue());
        AddField("accessibility", accessibility);

        return formFields;
    }
}