using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Dependencies, sitting between the public
///     <c>IDependenciesClient</c> controller and <c>IDependenciesRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IDependenciesService
{
    IAsyncEnumerable<GitLabDependency> ListAsync(ProjectId projectId, DependencyListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabDependencyListExport> CreateProjectExportAsync(ProjectId projectId,
        CreateProjectDependencyListExportRequest request, CancellationToken cancellationToken = default);

    Task<GitLabDependencyListExport> CreateGroupExportAsync(GroupId groupId,
        CreateGroupDependencyListExportRequest request, CancellationToken cancellationToken = default);

    Task<GitLabDependencyListExport> CreatePipelineExportAsync(long pipelineId,
        CreatePipelineDependencyListExportRequest request, CancellationToken cancellationToken = default);

    Task<GitLabDependencyListExport> GetExportAsync(long exportId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadExportAsync(long exportId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDependencyVulnerability> ListOccurrenceVulnerabilitiesAsync(string occurrenceId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAttestation> ListAttestationsAsync(ProjectId projectId, string subjectDigest,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadAttestationAsync(ProjectId projectId, long attestationIid,
        CancellationToken cancellationToken = default);

    Task<JsonElement> ScanFileAsync(ProjectId projectId, string sastEndpoint, SastFileScanRequest request,
        CancellationToken cancellationToken = default);

    Task AuthorizeSbomScanUploadAsync(long jobId, AuthorizeSbomScanUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabSbomScan> UploadSbomScanAsync(long jobId, GitLabFileUpload file, string? sbomDigest = null,
        CancellationToken cancellationToken = default);

    Task<GitLabSbomScan> GetSbomScanAsync(long jobId, string sbomDigest,
        CancellationToken cancellationToken = default);

    Task<GitLabSbomScan> ReuseSbomScanAsync(long jobId, string sbomDigest, ReuseSbomScanRequest request,
        CancellationToken cancellationToken = default);
}