using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Jobs" API area (<c>/projects/:id/jobs</c>).</summary>
public interface IJobsClient
{
    /// <summary>
    ///     Lists a project's jobs (<c>GET /projects/:id/jobs</c>), streaming every page. Filter by status
    ///     or ref via <see cref="JobListOptions" />.
    /// </summary>
    IAsyncEnumerable<GitLabJob> ListAsync(ProjectId projectId, JobListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the jobs that belong to one pipeline (<c>GET /projects/:id/pipelines/:pipeline_id/jobs</c>).</summary>
    IAsyncEnumerable<GitLabJob> ListForPipelineAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single job (<c>GET /projects/:id/jobs/:job_id</c>).</summary>
    Task<GitLabJob> GetAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default);

    /// <summary>Cancels a running or pending job (<c>POST /projects/:id/jobs/:job_id/cancel</c>).</summary>
    Task<GitLabJob> CancelAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retries a finished job (<c>POST /projects/:id/jobs/:job_id/retry</c>), creating a new job that
    ///     re-runs it.
    /// </summary>
    Task<GitLabJob> RetryAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Runs a manual job (<c>POST /projects/:id/jobs/:job_id/play</c>) - the action behind the "play"
    ///     button on a job that waits for someone to trigger it.
    /// </summary>
    Task<GitLabJob> PlayAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Erases a job: GitLab removes its artifacts and its log, and returns the job with
    ///     <c>erased_at</c> set.
    /// </summary>
    Task<GitLabJob> EraseAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the job the calling CI/CD job token belongs to (<c>GET /job</c>). Only meaningful when
    ///     the client is authenticated with a job token, which is how a running job introspects itself.
    /// </summary>
    Task<GitLabJob> GetCurrentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a job's log (<c>GET /projects/:id/jobs/:job_id/trace</c>) as plain text, streamed rather
    ///     than buffered - a log for a long build is routinely tens of megabytes.
    ///     <para>
    ///         The returned <see cref="GitLabFileResponse" /> owns the HTTP response and the connection, so
    ///         the caller MUST dispose it - <c>await using</c> - once the body has been read.
    ///     </para>
    /// </summary>
    Task<GitLabFileResponse> GetTraceAsync(ProjectId projectId, long jobId, JobTraceOptions? options = null,
        CancellationToken cancellationToken = default);

    // The runner protocol: GitLab Runner's own registration/reporting endpoints, scoped by a bare job or
    // runner id rather than by project. An application client has little reason to call these directly
    // unless it is implementing a custom runner.

    /// <summary>
    ///     Requests the next job for a runner to execute (<c>POST /jobs/request</c>). The response shape is
    ///     large, deeply nested and runner-version-dependent - the spec itself types nearly every leaf as a
    ///     bare string regardless of the field's real meaning - so it is surfaced as a raw
    ///     <see cref="JsonElement" /> rather than as an invented strongly-typed schema. GitLab answers 204
    ///     No Content when no job is available; that is not representable through this call today, since
    ///     the underlying <c>PostAsync&lt;TRequest, TResponse&gt;</c> always expects a body.
    /// </summary>
    Task<JsonElement> RequestAsync(JobRequestRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a job's state (<c>PUT /jobs/:id</c>) - how a runner reports progress, completion or
    ///     failure back to GitLab.
    /// </summary>
    Task UpdateAsync(long jobId, UpdateJobStateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a job's artifacts by job token (<c>GET /jobs/:id/artifacts</c>), with no project or
    ///     user authentication involved - this is how a runner or a downstream job fetches artifacts using
    ///     only the job's own token. Contrast with <see cref="IJobArtifactsClient.DownloadAsync" />, which
    ///     is project- and user-scoped.
    ///     <para>
    ///         The returned <see cref="GitLabFileResponse" /> owns the HTTP response and the connection, so
    ///         the caller MUST dispose it - <c>await using</c> - once the body has been read.
    ///     </para>
    /// </summary>
    Task<GitLabFileResponse> DownloadArtifactsByTokenAsync(long jobId,
        JobArtifactsByTokenDownloadOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a job's artifacts (<c>POST /jobs/:id/artifacts</c>) - the runner side of artifact
    ///     storage. <paramref name="file" /> becomes the multipart <c>file</c> part; the spec also allows a
    ///     second, optional binary <c>metadata</c> part, which cannot be represented here because the
    ///     transport's multipart upload carries at most one file part per request.
    /// </summary>
    Task UploadArtifactsAsync(long jobId, GitLabFileUpload file, string? token = null, string? expireIn = null,
        GitLabJobArtifactUploadType? artifactType = null, GitLabJobArtifactUploadFormat? artifactFormat = null,
        string? accessibility = null, CancellationToken cancellationToken = default);

    /// <summary>Authorizes an artifacts upload before the bytes are sent (<c>POST /jobs/:id/artifacts/authorize</c>).</summary>
    Task AuthorizeArtifactsUploadAsync(long jobId, AuthorizeJobArtifactsUploadRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Appends to a job's trace (<c>PATCH /jobs/:id/trace</c>). See
    ///     <see cref="AppendJobTraceRequest" /> for why this cannot carry the trace text itself.
    /// </summary>
    Task AppendTraceAsync(long jobId, AppendJobTraceRequest request, CancellationToken cancellationToken = default);
}