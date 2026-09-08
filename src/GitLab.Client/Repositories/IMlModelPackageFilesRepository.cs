using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the ML Model Registry's package file storage: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         This is a distinct tag, and a distinct route family, from <c>IMlModelsClient</c>: that
///         resource speaks GitLab's MLflow-compatible protocol under
///         <c>/projects/:id/ml/mlflow/api/2.0/mlflow/...</c> to manage model and version metadata, while
///         this one uploads and downloads the files attached to a model version, under
///         <c>/projects/:id/packages/ml_models/...</c> - the same package-registry storage layer every
///         other package format in GitLab shares.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IMlModelPackageFilesService), typeof(IMlModelPackageFilesClient))]
internal interface IMlModelPackageFilesRepository
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