using System.Globalization;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class GroupImportRepository(IGitLabApiConnection connection) : IGroupImportRepository
{
    public Task ImportAsync(GitLabFileUpload file, string path, string name, long? parentId = null,
        long? organizationId = null, CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> formFields = new(StringComparer.Ordinal) { ["path"] = path, ["name"] = name };

        if (parentId is { } parent)
        {
            formFields["parent_id"] = parent.ToString(CultureInfo.InvariantCulture);
        }

        if (organizationId is { } organization)
        {
            formFields["organization_id"] = organization.ToString(CultureInfo.InvariantCulture);
        }

        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("groups").Literal("import").Build(),
            file,
            formFields,
            cancellationToken);
    }

    public Task AuthorizeImportAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Literal("import").Literal("authorize").Build(),
            cancellationToken);
    }

    public Task CreateExportAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("export").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadExportAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("export").Literal("download").Build(),
            cancellationToken);
    }

    public Task ScheduleRelationsExportAsync(GroupId groupId, ScheduleGroupRelationsExportRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("export_relations").Build(),
            request,
            GitLabJsonContext.Default.ScheduleGroupRelationsExportRequest,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadRelationsExportAsync(GroupId groupId, string relation,
        bool? batched = null, int? batchNumber = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("export_relations").Literal("download")
                .Query("relation", relation)
                .Query("batched", batched)
                .Query("batch_number", batchNumber)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabGroupRelationsExportStatus> GetRelationsExportStatusAsync(GroupId groupId, string relation,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("export_relations").Literal("status")
                .Query("relation", relation)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupRelationsExportStatus,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroupRelationsExportStatus> ListRelationsExportStatusesAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("export_relations").Literal("status")
                .Build(),
            GitLabJsonContext.Default.GitLabGroupRelationsExportStatusArray,
            cancellationToken);
    }
}