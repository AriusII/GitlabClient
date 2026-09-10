using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class MlModelPackageFilesClient(IGitLabApiConnection connection) : IMlModelPackageFilesClient
{
    public Task<GitLabFileResponse> DownloadFileAsync(ProjectId projectId, long modelVersionId, string fileName,
        MlModelPackageFileStatusOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            FilesRoute(projectId, modelVersionId).Escaped(fileName).QueryFrom(options).Build(),
            cancellationToken);
    }

    public Task<JsonElement> UploadFileAsync(ProjectId projectId, long modelVersionId, string fileName,
        GitLabFileUpload file, GitLabPackageFileStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PutFileAsync(
            FilesRoute(projectId, modelVersionId).Escaped(fileName).Build(),
            file,
            BuildUploadFormFields(status),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task AuthorizeFileUploadAsync(ProjectId projectId, long modelVersionId, string fileName,
        AuthorizeMlModelPackageFileUploadRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            FilesRoute(projectId, modelVersionId).Escaped(fileName).Literal("authorize").Build(),
            request ?? new AuthorizeMlModelPackageFileUploadRequest(),
            GitLabJsonContext.Default.AuthorizeMlModelPackageFileUploadRequest,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadFileByPathAsync(ProjectId projectId, long modelVersionId, string path,
        string fileName, MlModelPackageFileStatusOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            FilesRoute(projectId, modelVersionId).Escaped(path).Escaped(fileName).QueryFrom(options).Build(),
            cancellationToken);
    }

    public Task<JsonElement> UploadFileByPathAsync(ProjectId projectId, long modelVersionId, string path,
        string fileName, GitLabFileUpload file, GitLabPackageFileStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PutFileAsync(
            FilesRoute(projectId, modelVersionId).Escaped(path).Escaped(fileName).Build(),
            file,
            BuildUploadFormFields(status),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task AuthorizeFileUploadByPathAsync(ProjectId projectId, long modelVersionId, string path,
        string fileName, AuthorizeMlModelPackageFileUploadRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            FilesRoute(projectId, modelVersionId).Escaped(path).Escaped(fileName).Literal("authorize").Build(),
            request ?? new AuthorizeMlModelPackageFileUploadRequest(),
            GitLabJsonContext.Default.AuthorizeMlModelPackageFileUploadRequest,
            cancellationToken);
    }

    private static GitLabRouteBuilder FilesRoute(ProjectId projectId, long modelVersionId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("packages")
            .Literal("ml_models")
            .Segment(modelVersionId)
            .Literal("files");
    }

    private static Dictionary<string, string>? BuildUploadFormFields(GitLabPackageFileStatus? status)
    {
        return status is null
            ? null
            : new Dictionary<string, string>(StringComparer.Ordinal) { ["status"] = status.Value.ToApiValue() };
    }
}