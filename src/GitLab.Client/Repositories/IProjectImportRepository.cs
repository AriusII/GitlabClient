using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Project import/export and Project templates resources: builds routes
///     via <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IProjectImportService), typeof(IProjectImportClient))]
internal interface IProjectImportRepository
{
    Task ExportAsync(ProjectId projectId, ExportProjectRequest? request = null,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectExportStatus> GetExportStatusAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadExportAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task ExportRelationsAsync(ProjectId projectId, ExportProjectRelationsRequest? request = null,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadRelationsExportAsync(ProjectId projectId, string relation,
        bool? batched = null, int? batchNumber = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProjectRelationExportStatus> ListRelationExportStatusesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectRelationExportStatus> GetRelationExportStatusAsync(ProjectId projectId, string relation,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectImportStatus> GetImportStatusAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectImportStatus> ImportFromGitAsync(ProjectId projectId, ImportProjectFromGitRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectImportStatus> GetRelationImportStatusAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectImportStatus> ImportArchiveAsync(GitLabFileUpload archive, ImportProjectArchiveRequest request,
        CancellationToken cancellationToken = default);

    Task AuthorizeArchiveUploadAsync(CancellationToken cancellationToken = default);

    Task<GitLabProjectRelationImport> ImportRelationAsync(GitLabFileUpload archive,
        ImportProjectRelationRequest request, CancellationToken cancellationToken = default);

    Task AuthorizeRelationUploadAsync(CancellationToken cancellationToken = default);

    Task<GitLabProjectImportStatus> ImportFromRemoteArchiveAsync(ImportProjectFromRemoteArchiveRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectImportStatus> ImportFromS3Async(ImportProjectFromS3Request request,
        CancellationToken cancellationToken = default);

    Task<GitLabImportedProject> ImportFromGitHubAsync(ImportProjectFromGitHubRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabRemoteImportedProject> CancelGitHubImportAsync(CancelGitHubImportRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabRemoteImportedProject> ImportFromBitbucketAsync(ImportProjectFromBitbucketRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabImportedProject> ImportFromBitbucketServerAsync(ImportProjectFromBitbucketServerRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProjectTemplate> ListTemplatesAsync(ProjectId projectId, GitLabProjectTemplateType type,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectTemplateDetail> GetTemplateAsync(ProjectId projectId, GitLabProjectTemplateType type,
        string name, ProjectTemplateOptions? options = null, CancellationToken cancellationToken = default);
}