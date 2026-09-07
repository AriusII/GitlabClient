using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Group import and export, sitting between the public
///     <c>IGroupImportClient</c> controller and <c>IGroupImportRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IGroupImportService
{
    Task ImportAsync(GitLabFileUpload file, string path, string name, long? parentId = null,
        long? organizationId = null, CancellationToken cancellationToken = default);

    Task AuthorizeImportAsync(CancellationToken cancellationToken = default);

    Task CreateExportAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadExportAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task ScheduleRelationsExportAsync(GroupId groupId, ScheduleGroupRelationsExportRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadRelationsExportAsync(GroupId groupId, string relation, bool? batched = null,
        int? batchNumber = null, CancellationToken cancellationToken = default);

    Task<GitLabGroupRelationsExportStatus> GetRelationsExportStatusAsync(GroupId groupId, string relation,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroupRelationsExportStatus> ListRelationsExportStatusesAsync(GroupId groupId,
        CancellationToken cancellationToken = default);
}