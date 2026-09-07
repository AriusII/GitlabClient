using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Group import and export resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IGroupImportService), typeof(IGroupImportClient))]
internal interface IGroupImportRepository
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