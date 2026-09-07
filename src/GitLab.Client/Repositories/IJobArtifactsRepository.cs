using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the JobArtifacts resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IJobArtifactsService), typeof(IJobArtifactsClient))]
internal interface IJobArtifactsRepository
{
    /// <summary>
    ///     Downloads a job's artifacts (<c>GET /projects/:id/jobs/:job_id/artifacts</c>) - the whole archive
    ///     by default, or one report when <see cref="JobArtifactDownloadOptions.FileType" /> is set.
    ///     <para>
    ///         The returned <see cref="GitLabFileResponse" /> owns the HTTP response and the connection, so
    ///         the caller MUST dispose it - <c>await using</c> - once the body has been read.
    ///         <see cref="GitLabFileResponse.FileName" /> carries the name GitLab sent in
    ///         <c>Content-Disposition</c>, normally <c>artifacts.zip</c>.
    ///     </para>
    /// </summary>
    Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, long jobId,
        JobArtifactDownloadOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one file out of a job's artifacts archive without fetching the archive
    ///     (<c>GET /projects/:id/jobs/:job_id/artifacts/:artifact_path</c>). Discover the path with
    ///     <see cref="ListAsync" />.
    ///     <para>
    ///         <paramref name="artifactPath" /> is percent-encoded on the way out, so a nested path such as
    ///         <c>coverage/index.html</c> is safe to pass verbatim. The returned
    ///         <see cref="GitLabFileResponse" /> must be disposed by the caller.
    ///     </para>
    /// </summary>
    Task<GitLabFileResponse> DownloadFileAsync(ProjectId projectId, long jobId, string artifactPath,
        string? jobToken = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads the artifacts of the latest successful run of a named job on a branch or tag
    ///     (<c>GET /projects/:id/jobs/artifacts/:ref_name/download</c>) - the "latest artifacts for main"
    ///     download, which needs no job id.
    ///     <para>
    ///         <paramref name="refName" /> is percent-encoded on the way out, so a slash-bearing branch name
    ///         such as <c>release/1.0</c> stays one route parameter. The returned
    ///         <see cref="GitLabFileResponse" /> must be disposed by the caller.
    ///     </para>
    /// </summary>
    Task<GitLabFileResponse> DownloadForRefAsync(ProjectId projectId, string refName, string jobName,
        JobArtifactRefDownloadOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one file out of the artifacts of the latest successful run of a named job on a branch or
    ///     tag (<c>GET /projects/:id/jobs/artifacts/:ref_name/raw/:artifact_path</c>).
    ///     <para>
    ///         Both <paramref name="refName" /> and <paramref name="artifactPath" /> are percent-encoded on
    ///         the way out. The returned <see cref="GitLabFileResponse" /> must be disposed by the caller.
    ///     </para>
    /// </summary>
    Task<GitLabFileResponse> DownloadFileForRefAsync(ProjectId projectId, string refName, string artifactPath,
        string jobName, JobArtifactRefDownloadOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the files inside a job's artifacts archive
    ///     (<c>GET /projects/:id/jobs/:job_id/artifacts/tree</c>), streaming every page. This is how a caller
    ///     discovers the <c>artifactPath</c> for a single-file download.
    /// </summary>
    IAsyncEnumerable<GitLabJobArtifactEntry> ListAsync(ProjectId projectId, long jobId,
        JobArtifactTreeListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Exempts a job's artifacts from expiry (<c>POST /projects/:id/jobs/:job_id/artifacts/keep</c>) and
    ///     returns the job with <c>artifacts_expire_at</c> cleared.
    /// </summary>
    Task<GitLabJob> KeepAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default);

    /// <summary>Deletes one job's artifacts (<c>DELETE /projects/:id/jobs/:job_id/artifacts</c>).</summary>
    Task DeleteAsync(ProjectId projectId, long jobId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes the artifacts of every job in a project (<c>DELETE /projects/:id/artifacts</c>). GitLab
    ///     answers 202 and does the deletion in the background, so the artifacts are not gone when this
    ///     returns. Requires the Maintainer role.
    /// </summary>
    Task DeleteAllAsync(ProjectId projectId, CancellationToken cancellationToken = default);
}