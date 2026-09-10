using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

/// <summary>The direct, allocation-conscious transport projection of GitLab's Vulnerabilities OpenAPI tag.</summary>
internal sealed class VulnerabilitiesClient(IGitLabApiConnection connection) : IVulnerabilitiesClient
{
    public IAsyncEnumerable<GitLabVulnerability> ListAsync(ProjectId projectId,
        VulnerabilityListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectVulnerabilitiesRoute(projectId).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabVulnerabilityArray,
            cancellationToken);
    }

    public Task<GitLabVulnerability> CreateAsync(ProjectId projectId, CreateVulnerabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectVulnerabilitiesRoute(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreateVulnerabilityRequest,
            GitLabJsonContext.Default.GitLabVulnerability,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabVulnerabilityFinding> ListFindingsAsync(ProjectId projectId,
        VulnerabilityFindingListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("vulnerability_findings")
                .QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabVulnerabilityFindingArray,
            cancellationToken);
    }

    public Task<GitLabVulnerability> GetAsync(long vulnerabilityId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            VulnerabilityRoute(vulnerabilityId).Build(),
            GitLabJsonContext.Default.GitLabVulnerability,
            cancellationToken);
    }

    public Task<GitLabVulnerability> ConfirmAsync(long vulnerabilityId, VulnerabilityCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostLifecycleActionAsync(vulnerabilityId, "confirm", request, cancellationToken);
    }

    public Task<GitLabVulnerability> DismissAsync(long vulnerabilityId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            VulnerabilityRoute(vulnerabilityId).Literal("dismiss").Build(),
            GitLabJsonContext.Default.GitLabVulnerability,
            cancellationToken);
    }

    public Task<GitLabVulnerability> ResolveAsync(long vulnerabilityId, VulnerabilityCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostLifecycleActionAsync(vulnerabilityId, "resolve", request, cancellationToken);
    }

    public Task<GitLabVulnerability> RevertAsync(long vulnerabilityId, VulnerabilityCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostLifecycleActionAsync(vulnerabilityId, "revert", request, cancellationToken);
    }

    public Task UpdateAiDetectionAsync(long vulnerabilityId, UpdateVulnerabilityAiDetectionRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            VulnerabilityRoute(vulnerabilityId).Literal("flags").Literal("ai_detection").Build(),
            request,
            GitLabJsonContext.Default.UpdateVulnerabilityAiDetectionRequest,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabVulnerabilityRelatedIssue> ListIssueLinksAsync(long vulnerabilityId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            VulnerabilityRoute(vulnerabilityId).Literal("issue_links").Build(),
            GitLabJsonContext.Default.GitLabVulnerabilityRelatedIssueArray,
            cancellationToken);
    }

    public Task<GitLabVulnerabilityIssueLink> CreateIssueLinkAsync(long vulnerabilityId,
        CreateVulnerabilityIssueLinkRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            VulnerabilityRoute(vulnerabilityId).Literal("issue_links").Build(),
            request,
            GitLabJsonContext.Default.CreateVulnerabilityIssueLinkRequest,
            GitLabJsonContext.Default.GitLabVulnerabilityIssueLink,
            cancellationToken);
    }

    public Task DeleteIssueLinkAsync(long vulnerabilityId, long issueLinkId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            VulnerabilityRoute(vulnerabilityId).Literal("issue_links").Segment(issueLinkId).Build(),
            cancellationToken);
    }

    public Task<GitLabVulnerabilityExport> CreateGroupExportAsync(GroupId groupId,
        CreateVulnerabilityExportRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("security").Literal("groups").Segment(groupId)
                .Literal("vulnerability_exports").Build(),
            request,
            GitLabJsonContext.Default.CreateVulnerabilityExportRequest,
            GitLabJsonContext.Default.GitLabVulnerabilityExport,
            cancellationToken);
    }

    public Task<GitLabVulnerabilityExport> CreateProjectExportAsync(ProjectId projectId,
        CreateVulnerabilityExportRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("security").Literal("projects").Segment(projectId)
                .Literal("vulnerability_exports").Build(),
            request,
            GitLabJsonContext.Default.CreateVulnerabilityExportRequest,
            GitLabJsonContext.Default.GitLabVulnerabilityExport,
            cancellationToken);
    }

    public Task<GitLabVulnerabilityArchiveExport> CreateProjectArchiveExportAsync(ProjectId projectId,
        CreateVulnerabilityArchiveExportRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("security").Literal("projects").Segment(projectId)
                .Literal("vulnerability_archive_exports").Build(),
            request,
            GitLabJsonContext.Default.CreateVulnerabilityArchiveExportRequest,
            GitLabJsonContext.Default.GitLabVulnerabilityArchiveExport,
            cancellationToken);
    }

    public Task<GitLabVulnerabilityArchiveExport> GetArchiveExportAsync(long exportId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("security").Literal("vulnerability_archive_exports").Segment(exportId)
                .Build(),
            GitLabJsonContext.Default.GitLabVulnerabilityArchiveExport,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadArchiveExportAsync(long exportId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("security").Literal("vulnerability_archive_exports").Segment(exportId)
                .Literal("download").Build(),
            cancellationToken);
    }

    public Task<GitLabVulnerabilityExport> CreateInstanceExportAsync(CreateVulnerabilityExportRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("security").Literal("vulnerability_exports").Build(),
            request,
            GitLabJsonContext.Default.CreateVulnerabilityExportRequest,
            GitLabJsonContext.Default.GitLabVulnerabilityExport,
            cancellationToken);
    }

    public Task<GitLabVulnerabilityExport> GetExportAsync(long exportId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("security").Literal("vulnerability_exports").Segment(exportId).Build(),
            GitLabJsonContext.Default.GitLabVulnerabilityExport,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadExportAsync(long exportId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("security").Literal("vulnerability_exports").Segment(exportId)
                .Literal("download").Build(),
            cancellationToken);
    }

    private Task<GitLabVulnerability> PostLifecycleActionAsync(long vulnerabilityId, string action,
        VulnerabilityCommentRequest request, CancellationToken cancellationToken)
    {
        return connection.PostAsync(
            VulnerabilityRoute(vulnerabilityId).Literal(action).Build(),
            request,
            GitLabJsonContext.Default.VulnerabilityCommentRequest,
            GitLabJsonContext.Default.GitLabVulnerability,
            cancellationToken);
    }

    private static GitLabRouteBuilder ProjectVulnerabilitiesRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("vulnerabilities");
    }

    private static GitLabRouteBuilder VulnerabilityRoute(long vulnerabilityId)
    {
        return GitLabRouteBuilder.Create("vulnerabilities").Segment(vulnerabilityId);
    }
}