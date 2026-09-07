using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Secure files" API area (<c>/projects/:id/secure_files</c>) - certificates,
///     provisioning profiles and keystores stored per project and pulled into a job by the
///     <c>download-secure-files</c> tool rather than committed to the repository.
/// </summary>
public interface ISecureFilesClient
{
    /// <summary>Streams the metadata of every secure file on the project. Never the file contents.</summary>
    IAsyncEnumerable<GitLabSecureFile> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a new secure file to the project as <c>multipart/form-data</c>. GitLab enforces one file
    ///     per project per <paramref name="name" /> - a second upload under the same name is rejected rather
    ///     than replacing the first.
    /// </summary>
    /// <param name="projectId">The project to upload to.</param>
    /// <param name="name">The file name GitLab stores the upload under, sent as the <c>name</c> form field.</param>
    /// <param name="file">The file part. Its stream is read but never disposed, so the caller keeps ownership of it.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabSecureFile> CreateAsync(ProjectId projectId, string name, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one secure file's metadata - checksum, expiry and whatever GitLab parsed out of it.</summary>
    Task<GitLabSecureFile> GetAsync(ProjectId projectId, long secureFileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a secure file's raw contents.
    /// </summary>
    /// <returns>
    ///     The open body plus its media type, length and file name. The caller owns it and must
    ///     <c>await using</c> it - it holds the HTTP response and its connection open until disposed.
    /// </returns>
    Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, long secureFileId,
        CancellationToken cancellationToken = default);

    /// <summary>Permanently deletes a secure file. GitLab answers <c>204</c> and the contents are unrecoverable.</summary>
    Task DeleteAsync(ProjectId projectId, long secureFileId, CancellationToken cancellationToken = default);
}