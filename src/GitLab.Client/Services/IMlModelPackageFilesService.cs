using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for ML Model Registry package files, sitting between the public
///     <c>IMlModelPackageFilesClient</c> controller and <c>IMlModelPackageFilesRepository</c>'s raw
///     GitLab access. Mirrors the repository's method shapes 1:1 today (its implementation is
///     generated); this is the seam where request validation, caching, or cross-resource composition
///     would go once the resource needs more than pass-through.
/// </summary>
internal interface IMlModelPackageFilesService
{
    /// <summary>Downloads an ml_model package file (<c>GET .../packages/ml_models/:model_version_id/files/:file_name</c>).</summary>
    Task<GitLabFileResponse> DownloadFileAsync(ProjectId projectId, long modelVersionId, string fileName,
        MlModelPackageFileStatusOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads an ml_model package file
    ///     (<c>PUT .../packages/ml_models/:model_version_id/files/:file_name</c>). The spec captures no
    ///     response schema for this endpoint, so - as with <c>IPackagesNpmClient.PublishForProjectAsync</c> -
    ///     the answer is surfaced as a raw <see cref="JsonElement" /> rather than as an invented DTO.
    /// </summary>
    Task<JsonElement> UploadFileAsync(ProjectId projectId, long modelVersionId, string fileName,
        GitLabFileUpload file, GitLabPackageFileStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Authorizes an ml_model package file upload before the bytes are sent
    ///     (<c>PUT .../packages/ml_models/:model_version_id/files/:file_name/authorize</c>).
    /// </summary>
    Task AuthorizeFileUploadAsync(ProjectId projectId, long modelVersionId, string fileName,
        AuthorizeMlModelPackageFileUploadRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads an ml_model package file stored under a directory path
    ///     (<c>GET .../packages/ml_models/:model_version_id/files/:path/:file_name</c>).
    /// </summary>
    Task<GitLabFileResponse> DownloadFileByPathAsync(ProjectId projectId, long modelVersionId, string path,
        string fileName, MlModelPackageFileStatusOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads an ml_model package file under a directory path
    ///     (<c>PUT .../packages/ml_models/:model_version_id/files/:path/:file_name</c>).
    /// </summary>
    Task<JsonElement> UploadFileByPathAsync(ProjectId projectId, long modelVersionId, string path, string fileName,
        GitLabFileUpload file, GitLabPackageFileStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Authorizes an ml_model package file upload under a directory path before the bytes are sent
    ///     (<c>PUT .../packages/ml_models/:model_version_id/files/:path/:file_name/authorize</c>).
    /// </summary>
    Task AuthorizeFileUploadByPathAsync(ProjectId projectId, long modelVersionId, string path, string fileName,
        AuthorizeMlModelPackageFileUploadRequest? request = null, CancellationToken cancellationToken = default);
}