using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab project uploads API area (<c>/projects/:id/uploads</c>) - the markdown attachment
///     store an issue, merge request or comment embeds images and files from.
///     <para>
///         Every upload is addressable two ways, and GitLab exposes both: by its numeric id, and by the
///         32-character secret plus file name that appear in the markdown link. The <c>BySecret</c> methods
///         are the pair to use when all you have is a URL scraped out of a description.
///     </para>
///     <para>
///         Listing and deleting need the Maintainer or Owner role; downloading needs only read access to
///         the project.
///     </para>
/// </summary>
public interface IProjectUploadsClient
{
    /// <summary>
    ///     Streams every upload on the project, newest first. Requires the Maintainer or Owner role - this
    ///     is the administrative view, and the entries carry no link to the files themselves.
    /// </summary>
    IAsyncEnumerable<GitLabProjectUpload> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a file to the project's attachment store and returns the markdown that embeds it.
    /// </summary>
    /// <param name="projectId">The project to upload to.</param>
    /// <param name="file">
    ///     The file part. Its stream is read but never disposed, so the caller keeps ownership of it.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>Where the file landed, and the pastable markdown snippet.</returns>
    Task<GitLabProjectUploadLink> UploadAsync(ProjectId projectId, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asks GitLab Workhorse to authorize an upload before it is sent, which is how a very large
    ///     attachment is streamed straight to object storage instead of through Rails.
    ///     <para>
    ///         The reply is Workhorse's internal routing document, which the API does not declare a schema
    ///         for and only Workhorse itself consumes, so it is not surfaced here. Ordinary callers want
    ///         <see cref="UploadAsync" />, which does the whole thing in one call.
    ///     </para>
    /// </summary>
    Task AuthorizeUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one upload by its numeric id. Requires the Maintainer or Owner role.
    /// </summary>
    /// <returns>
    ///     The open body plus its media type, length and file name. The caller owns it and must
    ///     <c>await using</c> it - it holds the HTTP response and its connection open until disposed.
    /// </returns>
    Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, long uploadId,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one upload by its numeric id. Requires the Maintainer or Owner role.</summary>
    Task DeleteAsync(ProjectId projectId, long uploadId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one upload by the secret and file name from its markdown link. Needs only read access
    ///     to the project, unlike <see cref="DownloadAsync" />.
    /// </summary>
    /// <param name="projectId">The project the file was uploaded to.</param>
    /// <param name="secret">The 32-character secret from the upload's URL. Passed raw; it is encoded for you.</param>
    /// <param name="filename">The stored file name. Passed raw - spaces and other URL-unsafe characters included.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The open body. The caller owns it and must <c>await using</c> it.
    /// </returns>
    Task<GitLabFileResponse> DownloadBySecretAsync(ProjectId projectId, string secret, string filename,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes one upload by the secret and file name from its markdown link. Requires the Maintainer or
    ///     Owner role.
    /// </summary>
    Task DeleteBySecretAsync(ProjectId projectId, string secret, string filename,
        CancellationToken cancellationToken = default);
}