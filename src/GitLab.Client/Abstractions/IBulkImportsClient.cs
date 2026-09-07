using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's migration APIs: direct transfer (<c>/bulk_imports</c>), offline transfer
///     (<c>/offline_exports</c>, <c>/offline_imports</c>) and the GitHub gist import
///     (<c>/import/github/gists</c>).
///     <para>
///         Direct transfer copies groups and projects straight from another GitLab instance over the
///         API - no archive, no download step - and is what GitLab recommends over the file-based
///         export/import in <see cref="IGroupImportClient" />. Offline transfer is the same machinery with
///         an object storage bucket in the middle, for instances that cannot reach each other.
///     </para>
///     <para>
///         A migration is asynchronous and fans out: <see cref="CreateAsync" /> answers immediately with a
///         <see cref="GitLabBulkImport" />, and progress is read per entity through
///         <see cref="ListEntitiesForImportAsync" />. A migration can reach
///         <see cref="GitLabBulkImportStatus.Finished" /> and still have skipped records, so check
///         <see cref="GitLabBulkImport.HasFailures" /> and read
///         <see cref="ListEntityFailuresAsync" /> before calling a run clean.
///     </para>
/// </summary>
public interface IBulkImportsClient
{
    /// <summary>Streams every direct-transfer migration visible to the caller (<c>GET /bulk_imports</c>).</summary>
    IAsyncEnumerable<GitLabBulkImport> ListAsync(BulkImportListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts a direct-transfer migration (<c>POST /bulk_imports</c>).
    /// </summary>
    /// <param name="request">
    ///     The source instance and the entities to migrate. Its
    ///     <see cref="CreateBulkImportRequest.Configuration" /> carries an access token for the SOURCE
    ///     instance - the one credential in this library that is not the client's own - which is sent once
    ///     and never echoed back on any response.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The created migration, already in flight.</returns>
    Task<GitLabBulkImport> CreateAsync(CreateBulkImportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one direct-transfer migration (<c>GET /bulk_imports/:import_id</c>).</summary>
    Task<GitLabBulkImport> GetAsync(long importId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Cancels a running migration (<c>POST /bulk_imports/:import_id/cancel</c>) and returns it in its
    ///     new state. Cancelling does not roll back what has already been imported.
    /// </summary>
    Task<GitLabBulkImport> CancelAsync(long importId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the entities of every migration visible to the caller
    ///     (<c>GET /bulk_imports/entities</c>) - the instance-wide view across all migrations.
    /// </summary>
    IAsyncEnumerable<GitLabBulkImportEntity> ListEntitiesAsync(BulkImportEntityListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the entities of one migration
    ///     (<c>GET /bulk_imports/:import_id/entities</c>) - the per-group and per-project progress of a
    ///     single run.
    /// </summary>
    IAsyncEnumerable<GitLabBulkImportEntity> ListEntitiesForImportAsync(long importId,
        BulkImportEntityListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one entity of a migration
    ///     (<c>GET /bulk_imports/:import_id/entities/:entity_id</c>).
    /// </summary>
    Task<GitLabBulkImportEntity> GetEntityAsync(long importId, long entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every record a migration entity could not import
    ///     (<c>GET /bulk_imports/:import_id/entities/:entity_id/failures</c>), which is the complete list
    ///     that <see cref="GitLabBulkImportEntity.Failures" /> only samples.
    /// </summary>
    IAsyncEnumerable<GitLabBulkImportEntityFailure> ListEntityFailuresAsync(long importId, long entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Imports the authenticated user's GitHub gists into GitLab snippets
    ///     (<c>POST /import/github/gists</c>).
    /// </summary>
    /// <param name="request">The GitHub personal access token to read the gists with. Never echoed back.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    ///     A task that completes once GitLab has accepted the request. GitLab answers <c>202 Accepted</c>
    ///     with no body and runs the import in the background, emailing the user about any gist it skipped -
    ///     gists with more than ten files are skipped.
    /// </returns>
    Task ImportGitHubGistsAsync(ImportGitHubGistsRequest request, CancellationToken cancellationToken = default);

    /// <summary>Streams every offline transfer export (<c>GET /offline_exports</c>).</summary>
    IAsyncEnumerable<GitLabOfflineExport> ListOfflineExportsAsync(OfflineExportListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts an offline transfer export into an object storage bucket
    ///     (<c>POST /offline_exports</c>).
    /// </summary>
    /// <param name="request">The bucket, how to authenticate against it, and what to export.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    ///     A task that completes once GitLab has accepted the request. The pinned spec declares no response
    ///     entity for this operation, so nothing is deserialized; read the export back with
    ///     <see cref="ListOfflineExportsAsync" />.
    /// </returns>
    Task CreateOfflineExportAsync(CreateOfflineExportRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets one offline transfer export (<c>GET /offline_exports/:id</c>).</summary>
    Task<GitLabOfflineExport> GetOfflineExportAsync(long exportId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts an offline transfer import, reading a previously written export back out of object
    ///     storage (<c>POST /offline_imports</c>).
    /// </summary>
    /// <remarks>
    ///     The result is an ordinary migration: it appears in <see cref="ListAsync" />, its entities in
    ///     <see cref="ListEntitiesForImportAsync" />, and it can be cancelled with
    ///     <see cref="CancelAsync" />.
    /// </remarks>
    Task<GitLabBulkImport> CreateOfflineImportAsync(CreateOfflineImportRequest request,
        CancellationToken cancellationToken = default);
}