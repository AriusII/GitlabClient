using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class DependenciesRepository(IGitLabApiConnection connection) : IDependenciesRepository
{
    public IAsyncEnumerable<GitLabDependency> ListAsync(ProjectId projectId, DependencyListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("dependencies").QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabDependencyArray,
            cancellationToken);
    }

    public Task<GitLabDependencyListExport> CreateProjectExportAsync(ProjectId projectId,
        CreateProjectDependencyListExportRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("dependency_list_exports").Build(),
            request,
            GitLabJsonContext.Default.CreateProjectDependencyListExportRequest,
            GitLabJsonContext.Default.GitLabDependencyListExport,
            cancellationToken);
    }

    public Task<GitLabDependencyListExport> CreateGroupExportAsync(GroupId groupId,
        CreateGroupDependencyListExportRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("dependency_list_exports").Build(),
            request,
            GitLabJsonContext.Default.CreateGroupDependencyListExportRequest,
            GitLabJsonContext.Default.GitLabDependencyListExport,
            cancellationToken);
    }

    public Task<GitLabDependencyListExport> CreatePipelineExportAsync(long pipelineId,
        CreatePipelineDependencyListExportRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("pipelines").Segment(pipelineId).Literal("dependency_list_exports").Build(),
            request,
            GitLabJsonContext.Default.CreatePipelineDependencyListExportRequest,
            GitLabJsonContext.Default.GitLabDependencyListExport,
            cancellationToken);
    }

    public Task<GitLabDependencyListExport> GetExportAsync(long exportId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("dependency_list_exports").Segment(exportId).Build(),
            GitLabJsonContext.Default.GitLabDependencyListExport,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadExportAsync(long exportId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("dependency_list_exports").Segment(exportId).Literal("download").Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDependencyVulnerability> ListOccurrenceVulnerabilitiesAsync(string occurrenceId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("occurrences").Literal("vulnerabilities").Query("id", occurrenceId).Build(),
            GitLabJsonContext.Default.GitLabDependencyVulnerabilityArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAttestation> ListAttestationsAsync(ProjectId projectId, string subjectDigest,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("attestations")
                .Escaped(subjectDigest).Build(),
            GitLabJsonContext.Default.GitLabAttestationArray,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadAttestationAsync(ProjectId projectId, long attestationIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("attestations")
                .Segment(attestationIid).Literal("download").Build(),
            cancellationToken);
    }

    public Task<JsonElement> ScanFileAsync(ProjectId projectId, string sastEndpoint,
        SastFileScanRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("security_scans").Literal("sast")
                .Escaped(sastEndpoint).Build(),
            request,
            GitLabJsonContext.Default.SastFileScanRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task AuthorizeSbomScanUploadAsync(long jobId, AuthorizeSbomScanUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            SbomScansRoute(jobId).Literal("authorize").Build(),
            request,
            GitLabJsonContext.Default.AuthorizeSbomScanUploadRequest,
            cancellationToken);
    }

    public Task<GitLabSbomScan> UploadSbomScanAsync(long jobId, GitLabFileUpload file, string? sbomDigest = null,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string>? formFields = sbomDigest is null
            ? null
            : new Dictionary<string, string>(StringComparer.Ordinal) { ["sbom_digest"] = sbomDigest };

        return connection.PostFileAsync(
            SbomScansRoute(jobId).Build(),
            file,
            formFields,
            GitLabJsonContext.Default.GitLabSbomScan,
            cancellationToken);
    }

    public Task<GitLabSbomScan> GetSbomScanAsync(long jobId, string sbomDigest,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            SbomScansRoute(jobId).Escaped(sbomDigest).Build(),
            GitLabJsonContext.Default.GitLabSbomScan,
            cancellationToken);
    }

    public Task<GitLabSbomScan> ReuseSbomScanAsync(long jobId, string sbomDigest, ReuseSbomScanRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            SbomScansRoute(jobId).Escaped(sbomDigest).Build(),
            request,
            GitLabJsonContext.Default.ReuseSbomScanRequest,
            GitLabJsonContext.Default.GitLabSbomScan,
            cancellationToken);
    }

    private static GitLabRouteBuilder SbomScansRoute(long jobId)
    {
        return GitLabRouteBuilder.Create("jobs").Segment(jobId).Literal("sbom_scans");
    }
}