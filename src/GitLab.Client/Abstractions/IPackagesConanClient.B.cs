namespace GitLab.Client.Abstractions;

/// <summary>
///     Part B of the public Conan client: the instance-wide recipe file upload. Call
///     <see cref="IPackagesConanClient.AuthorizeRecipeFileUploadAsync" /> first, as GitLab's Workhorse
///     upload protocol requires, then upload the file itself with this method.
/// </summary>
public partial interface IPackagesConanClient
{
    /// <summary>
    ///     Uploads one recipe file (<c>conanfile.py</c>, <c>conanmanifest.txt</c>, <c>conan_sources.tgz</c>
    ///     or <c>conan_export.tgz</c>) to the instance-wide Conan registry. GitLab answers with no content;
    ///     the upload target must be the recipe revision returned by the upload-authorization step.
    /// </summary>
    Task UploadRecipeFileAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default);
}