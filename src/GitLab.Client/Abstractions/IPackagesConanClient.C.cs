namespace GitLab.Client.Abstractions;

/// <summary>
///     Part C of the public Conan client surface: the instance-wide package file upload that pairs with
///     <see cref="GetPackageUploadUrlsAsync" /> and <see cref="AuthorizePackageFileUploadAsync" />.
/// </summary>
public partial interface IPackagesConanClient
{
    /// <summary>
    ///     Uploads one package binary file (<c>conaninfo.txt</c>, <c>conanmanifest.txt</c> or
    ///     <c>conan_package.tgz</c>) to the package registry. Use the destination from
    ///     <see cref="GetPackageUploadUrlsAsync" />, and call <see cref="AuthorizePackageFileUploadAsync" />
    ///     first if the registry requires pre-upload authorization. GitLab answers with an empty body on
    ///     success.
    /// </summary>
    Task UploadPackageFileAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string conanPackageReference, string packageRevision,
        string fileName, GitLabFileUpload file, CancellationToken cancellationToken = default);
}