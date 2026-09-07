using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's software supply chain APIs: the dependency list
///     (<c>/projects/:id/dependencies</c>), the dependency list exports for a project, group or pipeline
///     (<c>/dependency_list_exports</c>), the SBOM scans a CI job uploads
///     (<c>/jobs/:id/sbom_scans</c>), provenance attestations (<c>/projects/:id/attestations</c>) and the
///     real-time SAST file scan (<c>/projects/:id/security_scans/sast</c>).
///     <para>
///         All of these are Ultimate-tier features. On a plan that does not include them GitLab answers
///         <c>403</c> or <c>404</c> rather than returning an empty list.
///     </para>
/// </summary>
public interface IDependenciesClient
{
    /// <summary>
    ///     Streams the dependencies the last dependency scanning job found in a project.
    ///     <see cref="GitLabDependency.Vulnerabilities" /> is present only when the caller may read the
    ///     project's vulnerabilities.
    /// </summary>
    IAsyncEnumerable<GitLabDependency> ListAsync(ProjectId projectId, DependencyListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Requests a dependency list export for a project. The export is generated asynchronously - poll
    ///     <see cref="GetExportAsync" /> until <see cref="GitLabDependencyListExport.HasFinished" /> is
    ///     <see langword="true" />, then call <see cref="DownloadExportAsync" />.
    /// </summary>
    Task<GitLabDependencyListExport> CreateProjectExportAsync(ProjectId projectId,
        CreateProjectDependencyListExportRequest request, CancellationToken cancellationToken = default);

    /// <summary>Requests a dependency list export covering every project in a group.</summary>
    Task<GitLabDependencyListExport> CreateGroupExportAsync(GroupId groupId,
        CreateGroupDependencyListExportRequest request, CancellationToken cancellationToken = default);

    /// <summary>Requests a merged SBOM export for the dependencies a single pipeline detected.</summary>
    Task<GitLabDependencyListExport> CreatePipelineExportAsync(long pipelineId,
        CreatePipelineDependencyListExportRequest request, CancellationToken cancellationToken = default);

    /// <summary>Polls an export until it reports having finished.</summary>
    Task<GitLabDependencyListExport> GetExportAsync(long exportId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a finished export. The body is the export file rather than JSON, so the caller owns the
    ///     returned <see cref="GitLabFileResponse" /> and must <see langword="await" />
    ///     <see langword="using" /> it.
    /// </summary>
    Task<GitLabFileResponse> DownloadExportAsync(long exportId, CancellationToken cancellationToken = default);

    /// <summary>Streams the vulnerabilities related to one dependency occurrence.</summary>
    IAsyncEnumerable<GitLabDependencyVulnerability> ListOccurrenceVulnerabilitiesAsync(string occurrenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the provenance attestations recorded for one artifact hash. GitLab gates these endpoints
    ///     behind a feature flag and calls them not ready for production use.
    /// </summary>
    IAsyncEnumerable<GitLabAttestation> ListAttestationsAsync(ProjectId projectId, string subjectDigest,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one attestation bundle by its per-project iid. The body is a bundle rather than JSON,
    ///     so the caller owns the returned <see cref="GitLabFileResponse" /> and must
    ///     <see langword="await" /> <see langword="using" /> it.
    /// </summary>
    Task<GitLabFileResponse> DownloadAttestationAsync(ProjectId projectId, long attestationIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Scans a single file for vulnerabilities and returns the SAST findings in real time, without a
    ///     pipeline.
    /// </summary>
    /// <remarks>
    ///     GitLab marks this endpoint an experiment and declares no response schema for it - the body is
    ///     whatever the configured scanner service produced. It is therefore surfaced as a raw
    ///     <see cref="JsonElement" /> rather than a DTO invented from a shape the spec does not promise, so
    ///     that a scanner changing its report format cannot turn a successful scan into a deserialization
    ///     failure.
    /// </remarks>
    Task<JsonElement> ScanFileAsync(ProjectId projectId, string sastEndpoint, SastFileScanRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks an SBOM upload against the instance's size limit before the file is sent. GitLab answers
    ///     <c>413</c> when the file is too large.
    /// </summary>
    Task AuthorizeSbomScanUploadAsync(long jobId, AuthorizeSbomScanUploadRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a job's software bill of materials to be scanned. The upload's stream is read but never
    ///     disposed here.
    /// </summary>
    Task<GitLabSbomScan> UploadSbomScanAsync(long jobId, GitLabFileUpload file, string? sbomDigest = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a job's SBOM scan by the digest of the uploaded document. GitLab answers <c>202</c> with no
    ///     body while the scan is still running.
    /// </summary>
    Task<GitLabSbomScan> GetSbomScanAsync(long jobId, string sbomDigest,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reuses the results of an existing scan for the same SBOM digest instead of uploading the document
    ///     again.
    /// </summary>
    Task<GitLabSbomScan> ReuseSbomScanAsync(long jobId, string sbomDigest, ReuseSbomScanRequest request,
        CancellationToken cancellationToken = default);
}