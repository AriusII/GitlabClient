using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Repositories;

/// <summary>
///     Part D: uploading the actual Debian package file, the paired operation to
///     <see cref="IPackagesDebianRepository.AuthorizePackageUploadAsync" />.
/// </summary>
internal partial interface IPackagesDebianRepository
{
    /// <summary>
    ///     Uploads a Debian package file to a project
    ///     (<c>PUT /projects/:id/packages/debian/:file_name</c>). GitLab answers with no body.
    /// </summary>
    Task UploadPackageFileAsync(ProjectId projectId, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default);
}