using GitLab.Client.Domain;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Part D: uploading a Debian package file, the paired operation to
///     <see cref="IPackagesDebianClient.AuthorizePackageUploadAsync" />.
/// </summary>
public partial interface IPackagesDebianClient
{
    /// <summary>
    ///     Uploads a Debian package file to a project (<c>PUT /projects/:id/packages/debian/:file_name</c>).
    ///     Callers typically run <see cref="AuthorizePackageUploadAsync" /> first, per GitLab's Workhorse
    ///     upload protocol. GitLab answers with <c>201 Created</c> and no body.
    /// </summary>
    Task UploadPackageFileAsync(ProjectId projectId, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default);
}