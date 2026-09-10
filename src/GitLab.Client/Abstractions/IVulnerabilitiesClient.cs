using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's vulnerability objects, vulnerability findings, lifecycle actions, issue relations and
///     asynchronous vulnerability-report exports.
/// </summary>
/// <remarks>
///     GitLab separates a vulnerability object from the scanner finding that produced it. Project
///     <see cref="ListFindingsAsync" /> results are the latter; <see cref="ListAsync" />, lifecycle methods,
///     issue links and exports operate on the former.
/// </remarks>
public interface IVulnerabilitiesClient
{
    /// <summary>Streams the vulnerability objects in a project (<c>GET /projects/:id/vulnerabilities</c>).</summary>
    IAsyncEnumerable<GitLabVulnerability> ListAsync(ProjectId projectId, VulnerabilityListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a vulnerability object from a confirmed project finding.</summary>
    Task<GitLabVulnerability> CreateAsync(ProjectId projectId, CreateVulnerabilityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams a project's scanner findings (<c>GET /projects/:id/vulnerability_findings</c>), including
    ///     dismissed findings when requested by <paramref name="options" />.
    /// </summary>
    IAsyncEnumerable<GitLabVulnerabilityFinding> ListFindingsAsync(ProjectId projectId,
        VulnerabilityFindingListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves one instance-wide vulnerability object by its global id.</summary>
    Task<GitLabVulnerability> GetAsync(long vulnerabilityId, CancellationToken cancellationToken = default);

    /// <summary>Marks a vulnerability as confirmed and records an optional audit comment.</summary>
    Task<GitLabVulnerability> ConfirmAsync(long vulnerabilityId, VulnerabilityCommentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Dismisses a vulnerability (<c>POST /vulnerabilities/:id/dismiss</c>).</summary>
    Task<GitLabVulnerability> DismissAsync(long vulnerabilityId, CancellationToken cancellationToken = default);

    /// <summary>Marks a vulnerability as resolved and records an optional audit comment.</summary>
    Task<GitLabVulnerability> ResolveAsync(long vulnerabilityId, VulnerabilityCommentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Reverts a vulnerability to the detected state and records an optional audit comment.</summary>
    Task<GitLabVulnerability> RevertAsync(long vulnerabilityId, VulnerabilityCommentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Stores AI false-positive-detection evidence for a vulnerability.</summary>
    Task UpdateAiDetectionAsync(long vulnerabilityId, UpdateVulnerabilityAiDetectionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the issues related to a vulnerability.</summary>
    IAsyncEnumerable<GitLabVulnerabilityRelatedIssue> ListIssueLinksAsync(long vulnerabilityId,
        CancellationToken cancellationToken = default);

    /// <summary>Relates an issue to a vulnerability.</summary>
    Task<GitLabVulnerabilityIssueLink> CreateIssueLinkAsync(long vulnerabilityId,
        CreateVulnerabilityIssueLinkRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a vulnerability/issue relationship. GitLab's response has a link body, but the shared
    ///     DELETE transport intentionally exposes successful deletion as a bodyless completion.
    /// </summary>
    Task DeleteIssueLinkAsync(long vulnerabilityId, long issueLinkId, CancellationToken cancellationToken = default);

    /// <summary>Starts an asynchronous vulnerability-report export for one group.</summary>
    Task<GitLabVulnerabilityExport> CreateGroupExportAsync(GroupId groupId, CreateVulnerabilityExportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Starts an asynchronous vulnerability-report export for one project.</summary>
    Task<GitLabVulnerabilityExport> CreateProjectExportAsync(ProjectId projectId,
        CreateVulnerabilityExportRequest request, CancellationToken cancellationToken = default);

    /// <summary>Starts an asynchronous CSV export of a project's archived vulnerabilities.</summary>
    Task<GitLabVulnerabilityArchiveExport> CreateProjectArchiveExportAsync(ProjectId projectId,
        CreateVulnerabilityArchiveExportRequest request, CancellationToken cancellationToken = default);

    /// <summary>Polls an archived-vulnerability export until its status is terminal.</summary>
    Task<GitLabVulnerabilityArchiveExport> GetArchiveExportAsync(long exportId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a finished archived-vulnerability export. The caller owns the returned response and must
    ///     asynchronously dispose it.
    /// </summary>
    Task<GitLabFileResponse> DownloadArchiveExportAsync(long exportId, CancellationToken cancellationToken = default);

    /// <summary>Starts an asynchronous vulnerability-report export for the current user's selected projects.</summary>
    Task<GitLabVulnerabilityExport> CreateInstanceExportAsync(CreateVulnerabilityExportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Polls a vulnerability-report export until its status is terminal.</summary>
    Task<GitLabVulnerabilityExport> GetExportAsync(long exportId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a finished vulnerability-report export. The caller owns the returned response and must
    ///     asynchronously dispose it.
    /// </summary>
    Task<GitLabFileResponse> DownloadExportAsync(long exportId, CancellationToken cancellationToken = default);
}