using GitLab.Client.Abstractions;

namespace GitLab.Client.Repositories;

/// <summary>
///     Part C of the Conan package registry's raw file transfer (v1): the instance-wide package file
///     upload that <c>PackagesConanRepository.Files.cs</c> deliberately left out before the transport
///     grew a no-content multipart <c>PUT</c>. See <c>PackagesConanRepository.C.cs</c> for the
///     implementation.
/// </summary>
internal partial interface IPackagesConanRepository
{
    Task UploadPackageFileAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string conanPackageReference, string packageRevision,
        string fileName, GitLabFileUpload file, CancellationToken cancellationToken = default);
}