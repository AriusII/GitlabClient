using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Project import" and "Project templates" API areas - moving a whole project into
///     or out of an instance (<c>/projects/:id/export</c>, <c>/projects/import</c>,
///     <c>/projects/remote-import</c>, <c>/import/github</c>, <c>/import/bitbucket</c>) and reading the
///     file templates a project exposes (<c>/projects/:id/templates/:type</c>).
///     <para>
///         Export and import are both asynchronous on the server: the schedule call returns immediately
///         and the work happens in the background. Poll <see cref="GetExportStatusAsync" /> until it reports
///         <see cref="GitLabProjectExportState.Finished" /> before calling
///         <see cref="DownloadExportAsync" />, and poll <see cref="GetImportStatusAsync" /> after any of the
///         import calls.
///     </para>
///     <para>
///         SECURITY: the request records for the external-forge imports carry credentials for the source
///         system - a GitHub or Bitbucket token, an AWS secret access key, a Git URL password. They exist
///         only to be sent to your GitLab instance over TLS. Do not log a populated request, do not put one
///         in an exception message, and do not persist one alongside ordinary request telemetry.
///     </para>
/// </summary>
public interface IProjectImportClient
{
    /// <summary>
    ///     Schedules an export of a project (<c>POST /projects/:id/export</c>). Returns as soon as GitLab
    ///     accepts the job; poll <see cref="GetExportStatusAsync" /> for progress.
    /// </summary>
    /// <param name="projectId">The project to export.</param>
    /// <param name="request">
    ///     Optional overrides - a replacement description, relations to exclude, or an upload destination
    ///     for the finished archive. Omit it to schedule a plain export.
    /// </param>
    /// <param name="cancellationToken">Cancels the request. Does not cancel an export GitLab has already accepted.</param>
    Task ExportAsync(ProjectId projectId, ExportProjectRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the state of the most recent export of a project (<c>GET /projects/:id/export</c>). Only
    ///     <see cref="GitLabProjectExportState.Finished" /> means the archive can be downloaded.
    /// </summary>
    Task<GitLabProjectExportStatus> GetExportStatusAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads the most recent project export archive
    ///     (<c>GET /projects/:id/export/download</c>) as a raw <c>tar.gz</c> body.
    /// </summary>
    /// <param name="projectId">The project whose archive to download.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The open archive stream, its length, and the file name GitLab put in <c>Content-Disposition</c>
    ///     (<see cref="GitLabFileResponse.FileName" />). THE CALLER OWNS IT and must dispose it -
    ///     <c>await using</c> - or the pooled connection it streams over is never returned.
    /// </returns>
    /// <exception cref="Exceptions.GitLabNotFoundException">
    ///     No finished export exists. GitLab answers <c>404</c> while the export is queued, running or
    ///     failed, so this does not mean the project is missing.
    /// </exception>
    Task<GitLabFileResponse> DownloadExportAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Schedules a relations export of a project (<c>POST /projects/:id/export_relations</c>) - the
    ///     "direct transfer" format, one NDJSON file per relation rather than a single archive.
    /// </summary>
    /// <param name="projectId">The project to export.</param>
    /// <param name="request">Optionally asks for a batched export. Omit for the unbatched default.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task ExportRelationsAsync(ProjectId projectId, ExportProjectRelationsRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one relation of a finished relations export
    ///     (<c>GET /projects/:id/export_relations/download</c>).
    /// </summary>
    /// <param name="projectId">The project whose relations export to read.</param>
    /// <param name="relation">The relation to download - <c>issues</c>, <c>merge_requests</c>, and so on.</param>
    /// <param name="batched">Set to true to download one batch of a batched export.</param>
    /// <param name="batchNumber">
    ///     Which batch to download, from
    ///     <see cref="GitLabProjectRelationExportBatch.BatchNumber" />. Only meaningful with
    ///     <paramref name="batched" />.
    /// </param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>The open body. THE CALLER OWNS IT and must dispose it - <c>await using</c>.</returns>
    Task<GitLabFileResponse> DownloadRelationsExportAsync(ProjectId projectId, string relation,
        bool? batched = null, int? batchNumber = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the export state of every relation of a project
    ///     (<c>GET /projects/:id/export_relations/status</c> with no <c>relation</c> filter).
    /// </summary>
    IAsyncEnumerable<GitLabProjectRelationExportStatus> ListRelationExportStatusesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the export state of one relation
    ///     (<c>GET /projects/:id/export_relations/status?relation=...</c>). Filtering by relation is what
    ///     turns this endpoint's answer from a list into a single object, which is why it is a separate
    ///     method from <see cref="ListRelationExportStatusesAsync" />.
    /// </summary>
    Task<GitLabProjectRelationExportStatus> GetRelationExportStatusAsync(ProjectId projectId, string relation,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the state of the most recent import into a project (<c>GET /projects/:id/import</c>) -
    ///     what you poll after any of the import calls below.
    /// </summary>
    Task<GitLabProjectImportStatus> GetImportStatusAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Imports a repository into an existing project from a Git URL
    ///     (<c>POST /projects/:id/import/git</c>).
    /// </summary>
    /// <param name="projectId">The project to import into.</param>
    /// <param name="request">
    ///     The source URL and, when it needs them, credentials for it. Treat a populated request as a
    ///     secret.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabProjectImportStatus> ImportFromGitAsync(ProjectId projectId, ImportProjectFromGitRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the state of the most recent single-relation import into a project
    ///     (<c>GET /projects/:id/relation-imports</c>). Only one relation import can be in flight at a time,
    ///     so this reports whether the previous <see cref="ImportRelationAsync" /> finished.
    /// </summary>
    Task<GitLabProjectImportStatus> GetRelationImportStatusAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a project by uploading an export archive (<c>POST /projects/import</c>), sent as
    ///     <c>multipart/form-data</c>.
    /// </summary>
    /// <param name="archive">
    ///     The <c>tar.gz</c> produced by <see cref="ExportAsync" />. Its stream is read but never disposed -
    ///     the caller keeps ownership.
    /// </param>
    /// <param name="request">Where the project should land, and any settings to override from the archive.</param>
    /// <param name="cancellationToken">Cancels the upload.</param>
    /// <returns>
    ///     The newly created project and its import state. The import itself runs in the background: poll
    ///     <see cref="GetImportStatusAsync" /> with the returned id.
    /// </returns>
    Task<GitLabProjectImportStatus> ImportArchiveAsync(GitLabFileUpload archive, ImportProjectArchiveRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Pre-authorizes an archive upload (<c>POST /projects/import/authorize</c>). This is the
    ///     Workhorse hand-off an ordinary API client does not need: <see cref="ImportArchiveAsync" /> posts
    ///     the file directly.
    /// </summary>
    Task AuthorizeArchiveUploadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replays one relation out of an export archive into an existing project
    ///     (<c>POST /projects/import-relation</c>), sent as <c>multipart/form-data</c>. Items already
    ///     imported are skipped.
    /// </summary>
    /// <param name="archive">The export archive to read the relation out of. Its stream is borrowed, never disposed.</param>
    /// <param name="request">The target project path and which relation to import.</param>
    /// <param name="cancellationToken">Cancels the upload.</param>
    Task<GitLabProjectRelationImport> ImportRelationAsync(GitLabFileUpload archive,
        ImportProjectRelationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Pre-authorizes a relation import upload (<c>POST /projects/import-relation/authorize</c>). The
    ///     Workhorse counterpart of <see cref="AuthorizeArchiveUploadAsync" />, and equally unnecessary for
    ///     an ordinary API client.
    /// </summary>
    Task AuthorizeRelationUploadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a project from an export archive GitLab downloads itself
    ///     (<c>POST /projects/remote-import</c>), rather than one you upload.
    /// </summary>
    /// <param name="request">
    ///     Where to fetch the archive from and where the project should land. The URL is typically
    ///     pre-signed and therefore a secret.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabProjectImportStatus> ImportFromRemoteArchiveAsync(ImportProjectFromRemoteArchiveRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a project from an export archive in an AWS S3 bucket
    ///     (<c>POST /projects/remote-import-s3</c>).
    /// </summary>
    /// <param name="request">Bucket coordinates plus the AWS credentials to read the object with. A secret.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabProjectImportStatus> ImportFromS3Async(ImportProjectFromS3Request request,
        CancellationToken cancellationToken = default);

    /// <summary>Imports a repository from GitHub (<c>POST /import/github</c>).</summary>
    /// <param name="request">
    ///     The GitHub repository id, the target namespace, and a GitHub personal access token. A secret.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    ///     The project GitLab created. Poll <see cref="GetImportStatusAsync" /> with its id for the
    ///     import's progress.
    /// </returns>
    Task<GitLabImportedProject> ImportFromGitHubAsync(ImportProjectFromGitHubRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Cancels an in-progress GitHub import (<c>POST /import/github/cancel</c>). Only works while the
    ///     import is scheduled or started.
    /// </summary>
    Task<GitLabRemoteImportedProject> CancelGitHubImportAsync(CancelGitHubImportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Imports a repository from Bitbucket Cloud (<c>POST /import/bitbucket</c>).</summary>
    /// <param name="request">
    ///     The repository path, the target namespace, and an Atlassian account plus API token. A secret.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabRemoteImportedProject> ImportFromBitbucketAsync(ImportProjectFromBitbucketRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Imports a repository from a self-managed Bitbucket Server
    ///     (<c>POST /import/bitbucket_server</c>).
    /// </summary>
    /// <param name="request">
    ///     The instance URL, project key and repository slug, plus a Bitbucket Server personal access
    ///     token. A secret.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabImportedProject> ImportFromBitbucketServerAsync(ImportProjectFromBitbucketServerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every template of one type available to a project
    ///     (<c>GET /projects/:id/templates/:type</c>) - the instance's built-in templates plus anything the
    ///     project's group contributes.
    /// </summary>
    /// <param name="projectId">The project to read templates for.</param>
    /// <param name="type">Which kind of template to list.</param>
    /// <param name="cancellationToken">Cancels the enumeration.</param>
    IAsyncEnumerable<GitLabProjectTemplate> ListTemplatesAsync(ProjectId projectId, GitLabProjectTemplateType type,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one template, including its body
    ///     (<c>GET /projects/:id/templates/:type/:name</c>).
    /// </summary>
    /// <param name="projectId">The project to read the template for.</param>
    /// <param name="type">Which kind of template to fetch.</param>
    /// <param name="name">
    ///     The template's key, as returned by <see cref="ListTemplatesAsync" />. Issue and merge request
    ///     template names legally contain <c>/</c>; the route builder percent-encodes the value, so pass it
    ///     raw.
    /// </param>
    /// <param name="options">
    ///     Optional placeholder expansion for licence templates, and a source project to disambiguate a
    ///     shared name.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabProjectTemplateDetail> GetTemplateAsync(ProjectId projectId, GitLabProjectTemplateType type,
        string name, ProjectTemplateOptions? options = null, CancellationToken cancellationToken = default);
}