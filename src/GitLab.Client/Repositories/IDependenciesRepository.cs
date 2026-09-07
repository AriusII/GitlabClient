using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Dependencies resource - dependency management, software
///     composition analysis, supply-chain attestations and the real-time SAST scan. Builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IDependenciesService), typeof(IDependenciesClient))]
internal interface IDependenciesRepository
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