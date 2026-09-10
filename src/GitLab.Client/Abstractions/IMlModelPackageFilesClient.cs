using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "ML Model Registry" API area
///     (<c>/projects/:id/packages/ml_models/...</c>) - uploading and downloading the files attached to a
///     version of a machine learning model. Model and model-version metadata itself is managed through
///     the MLflow-compatible protocol on <see cref="IMlModelsClient" /> instead.
/// </summary>
public interface IMlModelPackageFilesClient
{
    /// <summary>Downloads an ml_model package file (<c>GET .../packages/ml_models/:model_version_id/files/:file_name</c>).</summary>
    Task<GitLabFileResponse> DownloadFileAsync(ProjectId projectId, long modelVersionId, string fileName,
        MlModelPackageFileStatusOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads an ml_model package file
    ///     (<c>PUT .../packages/ml_models/:model_version_id/files/:file_name</c>). The spec captures no
    ///     response schema for this endpoint, so - as with <see cref="IPackagesNpmClient.PublishForProjectAsync" /> -
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