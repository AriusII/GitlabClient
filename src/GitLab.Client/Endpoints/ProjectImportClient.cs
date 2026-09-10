using System.Globalization;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ProjectImportClient(IGitLabApiConnection connection) : IProjectImportClient
{
    public Task ExportAsync(ProjectId projectId, ExportProjectRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        Uri route = GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("export").Build();

        // The body is optional here, and an empty one is the common case: send no payload at all rather
        // than an empty object, which Grape would still have to parse.
        return request is null
            ? connection.PostAsync(route, cancellationToken)
            : connection.PostAsync(route, request, GitLabJsonContext.Default.ExportProjectRequest,
                cancellationToken);
    }

    public Task<GitLabProjectExportStatus> GetExportStatusAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("export").Build(),
            GitLabJsonContext.Default.GitLabProjectExportStatus,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadExportAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("export").Literal("download").Build(),
            cancellationToken);
    }

    public Task ExportRelationsAsync(ProjectId projectId, ExportProjectRelationsRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        Uri route = GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("export_relations").Build();

        return request is null
            ? connection.PostAsync(route, cancellationToken)
            : connection.PostAsync(route, request, GitLabJsonContext.Default.ExportProjectRelationsRequest,
                cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadRelationsExportAsync(ProjectId projectId, string relation,
        bool? batched = null, int? batchNumber = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("export_relations").Literal("download")
                .Query("relation", relation)
                .Query("batched", batched)
                .Query("batch_number", batchNumber)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProjectRelationExportStatus> ListRelationExportStatusesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("export_relations").Literal("status")
                .Build(),
            GitLabJsonContext.Default.GitLabProjectRelationExportStatusArray,
            cancellationToken);
    }

    public Task<GitLabProjectRelationExportStatus> GetRelationExportStatusAsync(ProjectId projectId, string relation,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("export_relations").Literal("status")
                .Query("relation", relation)
                .Build(),
            GitLabJsonContext.Default.GitLabProjectRelationExportStatus,
            cancellationToken);
    }

    public Task<GitLabProjectImportStatus> GetImportStatusAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("import").Build(),
            GitLabJsonContext.Default.GitLabProjectImportStatus,
            cancellationToken);
    }

    public Task<GitLabProjectImportStatus> ImportFromGitAsync(ProjectId projectId,
        ImportProjectFromGitRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("import").Literal("git").Build(),
            request,
            GitLabJsonContext.Default.ImportProjectFromGitRequest,
            GitLabJsonContext.Default.GitLabProjectImportStatus,
            cancellationToken);
    }

    public async Task<IReadOnlyList<GitLabProjectRelationImport>> GetRelationImportStatusAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return await connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("relation-imports").Build(),
            GitLabJsonContext.Default.GitLabProjectRelationImportArray,
            cancellationToken).ConfigureAwait(false);
    }

    public Task<GitLabProjectImportStatus> ImportArchiveAsync(GitLabFileUpload archive,
        ImportProjectArchiveRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("projects").Literal("import").Build(),
            archive,
            BuildArchiveFormFields(request),
            GitLabJsonContext.Default.GitLabProjectImportStatus,
            cancellationToken);
    }

    public Task AuthorizeArchiveUploadAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Literal("import").Literal("authorize").Build(),
            cancellationToken);
    }

    public Task<GitLabProjectRelationImport> ImportRelationAsync(GitLabFileUpload archive,
        ImportProjectRelationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Dictionary<string, string> formFields = new(StringComparer.Ordinal)
        {
            ["path"] = request.Path, ["relation"] = request.Relation
        };

        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("projects").Literal("import-relation").Build(),
            archive,
            formFields,
            GitLabJsonContext.Default.GitLabProjectRelationImport,
            cancellationToken);
    }

    public Task AuthorizeRelationUploadAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Literal("import-relation").Literal("authorize").Build(),
            cancellationToken);
    }

    public Task<GitLabProjectImportStatus> ImportFromRemoteArchiveAsync(
        ImportProjectFromRemoteArchiveRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostMultipartFormAsync(
            GitLabRouteBuilder.Create("projects").Literal("remote-import").Build(),
            request,
            GitLabJsonContext.Default.ImportProjectFromRemoteArchiveRequest,
            GitLabJsonContext.Default.GitLabProjectImportStatus,
            cancellationToken);
    }

    public Task<GitLabProjectImportStatus> ImportFromS3Async(ImportProjectFromS3Request request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostMultipartFormAsync(
            GitLabRouteBuilder.Create("projects").Literal("remote-import-s3").Build(),
            request,
            GitLabJsonContext.Default.ImportProjectFromS3Request,
            GitLabJsonContext.Default.GitLabProjectImportStatus,
            cancellationToken);
    }

    public Task<GitLabImportedProject> ImportFromGitHubAsync(ImportProjectFromGitHubRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("import").Literal("github").Build(),
            request,
            GitLabJsonContext.Default.ImportProjectFromGitHubRequest,
            GitLabJsonContext.Default.GitLabImportedProject,
            cancellationToken);
    }

    public Task<GitLabRemoteImportedProject> CancelGitHubImportAsync(CancelGitHubImportRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("import").Literal("github").Literal("cancel").Build(),
            request,
            GitLabJsonContext.Default.CancelGitHubImportRequest,
            GitLabJsonContext.Default.GitLabRemoteImportedProject,
            cancellationToken);
    }

    public Task<GitLabRemoteImportedProject> ImportFromBitbucketAsync(ImportProjectFromBitbucketRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("import").Literal("bitbucket").Build(),
            request,
            GitLabJsonContext.Default.ImportProjectFromBitbucketRequest,
            GitLabJsonContext.Default.GitLabRemoteImportedProject,
            cancellationToken);
    }

    public Task<GitLabImportedProject> ImportFromBitbucketServerAsync(
        ImportProjectFromBitbucketServerRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("import").Literal("bitbucket_server").Build(),
            request,
            GitLabJsonContext.Default.ImportProjectFromBitbucketServerRequest,
            GitLabJsonContext.Default.GitLabImportedProject,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProjectTemplate> ListTemplatesAsync(ProjectId projectId,
        GitLabProjectTemplateType type, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("templates")
                .Literal(type.ToRouteValue()).Build(),
            GitLabJsonContext.Default.GitLabProjectTemplateArray,
            cancellationToken);
    }

    public Task<GitLabProjectTemplateDetail> GetTemplateAsync(ProjectId projectId, GitLabProjectTemplateType type,
        string name, ProjectTemplateOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("templates")
                .Literal(type.ToRouteValue()).Escaped(name).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabProjectTemplateDetail,
            cancellationToken);
    }

    /// <summary>
    ///     Flattens the non-file half of a project archive import onto multipart form fields. GitLab reads
    ///     nested parameters from bracketed field names, so <c>override_params</c> becomes one
    ///     <c>override_params[key]</c> field per entry rather than a JSON blob.
    /// </summary>
    private static Dictionary<string, string> BuildArchiveFormFields(ImportProjectArchiveRequest request)
    {
        Dictionary<string, string> formFields = new(StringComparer.Ordinal) { ["path"] = request.Path };

        if (request.Name is { } name)
        {
            formFields["name"] = name;
        }

        if (request.Namespace is { } @namespace)
        {
            formFields["namespace"] = @namespace;
        }

        if (request.NamespaceId is { } namespaceId)
        {
            formFields["namespace_id"] = namespaceId.ToString(CultureInfo.InvariantCulture);
        }

        if (request.NamespacePath is { } namespacePath)
        {
            formFields["namespace_path"] = namespacePath;
        }

        if (request.Overwrite is { } overwrite)
        {
            formFields["overwrite"] = overwrite ? "true" : "false";
        }

        if (request.OverrideParams is { } overrideParams)
        {
            foreach (KeyValuePair<string, string> parameter in overrideParams)
            {
                formFields[$"override_params[{parameter.Key}]"] = parameter.Value;
            }
        }

        return formFields;
    }
}