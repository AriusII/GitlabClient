using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Jobs, sitting between the public <c>IJobsClient</c>
///     controller and <c>IJobsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IJobsService
{
    IAsyncEnumerable<GitLabJob> ListAsync(ProjectId projectId, JobListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabJob> ListForPipelineAsync(ProjectId projectId, long pipelineId,
        CancellationToken cancellationToken = default);

    Task<GitLabJob> GetAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default);

    Task<GitLabJob> CancelAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default);

    Task<GitLabJob> RetryAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default);

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
}