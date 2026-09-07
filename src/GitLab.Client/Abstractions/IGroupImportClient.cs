using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Group import and export" API area (<c>/groups/import</c>,
///     <c>/groups/:id/export</c>, <c>/groups/:id/export_relations</c>) - moving a group between instances
///     as a file rather than over a live connection.
///     <para>
///         Two independent flows live here. The <em>file export</em> pair
///         (<see cref="CreateExportAsync" /> then <see cref="DownloadExportAsync" />) produces one archive
///         for the whole group, which <see cref="ImportAsync" /> uploads on the far side. The
///         <em>relations export</em> trio (<see cref="ScheduleRelationsExportAsync" />,
///         <see cref="ListRelationsExportStatusesAsync" />, <see cref="DownloadRelationsExportAsync" />)
///         splits the same data one relation at a time and is what direct transfer uses internally.
///     </para>
///     <para>
///         Every export endpoint is asynchronous: the POST answers <c>202 Accepted</c> and the archive
///         only becomes downloadable once GitLab has finished building it, so a
///         <see cref="Exceptions.GitLabNotFoundException" /> from a download shortly after scheduling
///         means "not ready yet", not "no such group".
///     </para>
///     <para>
///         For a live instance-to-instance migration, prefer <see cref="IBulkImportsClient" /> - GitLab
///         recommends direct transfer over file export/import.
///     </para>
/// </summary>
public interface IGroupImportClient
{
    /// <summary>
    ///     Uploads a group export archive to create a group (<c>POST /groups/import</c>), sending the
    ///     archive as <c>multipart/form-data</c>.
    /// </summary>
    /// <param name="file">
    ///     The archive produced by <see cref="DownloadExportAsync" /> on the source instance. Its stream is
    ///     read but not disposed - the caller keeps ownership.
    /// </param>
    /// <param name="path">The path to create the group at.</param>
    /// <param name="name">The name of the group to create.</param>
    /// <param name="parentId">
    ///     The group to nest the new group under. Defaults to the current user's namespace when omitted.
    /// </param>
    /// <param name="organizationId">The organization the new group belongs to.</param>
    /// <param name="cancellationToken">Cancels the upload.</param>
    /// <returns>
    ///     A task that completes once GitLab has accepted the archive. GitLab answers <c>202 Accepted</c>
    ///     with no body: the group is created in the background, so poll for the group itself to know when
    ///     the import finished.
    /// </returns>
    Task ImportAsync(GitLabFileUpload file, string path, string name, long? parentId = null,
        long? organizationId = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asks Workhorse to authorize a group import upload (<c>POST /groups/import/authorize</c>), the
    ///     pre-flight step of the accelerated upload path.
    /// </summary>
    /// <remarks>
    ///     <see cref="ImportAsync" /> does not need this: it posts the archive through the API directly.
    ///     This exists for callers reproducing GitLab's own two-step upload, and its Workhorse-internal
    ///     response body is deliberately not surfaced.
    /// </remarks>
    Task AuthorizeImportAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Schedules a group's file export (<c>POST /groups/:id/export</c>). Answers <c>202 Accepted</c>;
    ///     the archive is built in the background and fetched with <see cref="DownloadExportAsync" />.
    /// </summary>
    Task CreateExportAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads the archive produced by <see cref="CreateExportAsync" />
    ///     (<c>GET /groups/:id/export/download</c>).
    /// </summary>
    /// <param name="groupId">The group whose export to download.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The open archive body, with the server-supplied
    ///     <see cref="GitLabFileResponse.FileName" /> from <c>Content-Disposition</c>. The caller owns it:
    ///     <c>await using</c> it, or the pooled connection is never released.
    /// </returns>
    Task<GitLabFileResponse> DownloadExportAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Schedules a relations export for a group (<c>POST /groups/:id/export_relations</c>) - one file
    ///     per relation rather than one archive for the whole group.
    /// </summary>
    Task ScheduleRelationsExportAsync(GroupId groupId, ScheduleGroupRelationsExportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one relation of a relations export
    ///     (<c>GET /groups/:id/export_relations/download</c>).
    /// </summary>
    /// <param name="groupId">The group whose relations export to read.</param>
    /// <param name="relation">
    ///     The relation to download - <c>issues</c>, <c>labels</c>, <c>milestones</c>, ... The names are the
    ///     ones <see cref="ListRelationsExportStatusesAsync" /> reports.
    /// </param>
    /// <param name="batched">
    ///     Download a single batch rather than the whole relation. Only meaningful for an export scheduled
    ///     with <see cref="ScheduleGroupRelationsExportRequest.Batched" />.
    /// </param>
    /// <param name="batchNumber">Which batch to download, when <paramref name="batched" /> is set.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The open relation body, with the server-supplied
    ///     <see cref="GitLabFileResponse.FileName" /> from <c>Content-Disposition</c>. The caller owns it
    ///     and must <c>await using</c> it.
    /// </returns>
    Task<GitLabFileResponse> DownloadRelationsExportAsync(GroupId groupId, string relation, bool? batched = null,
        int? batchNumber = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the progress of one relation of a group's relations export
    ///     (<c>GET /groups/:id/export_relations/status?relation=...</c>).
    /// </summary>
    Task<GitLabGroupRelationsExportStatus> GetRelationsExportStatusAsync(GroupId groupId, string relation,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the progress of every relation of a group's relations export
    ///     (<c>GET /groups/:id/export_relations/status</c> with no <c>relation</c> filter, which GitLab
    ///     answers with the whole list).
    /// </summary>
    /// <remarks>
    ///     This and <see cref="GetRelationsExportStatusAsync" /> are the same GitLab operation: supplying
    ///     <c>relation</c> narrows the answer from a list to a single object, which is why the two response
    ///     shapes are exposed as two methods.
    /// </remarks>
    IAsyncEnumerable<GitLabGroupRelationsExportStatus> ListRelationsExportStatusesAsync(GroupId groupId,
        CancellationToken cancellationToken = default);
}