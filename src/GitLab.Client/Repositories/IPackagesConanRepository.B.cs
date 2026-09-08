using GitLab.Client.Abstractions;

namespace GitLab.Client.Repositories;

/// <summary>
///     Part B of the Conan repository interface: the instance-wide recipe file upload itself, the sibling
///     of <c>AuthorizeRecipeFileUploadAsync</c> that an earlier pass of this resource could not express
///     because the transport had no no-content multipart <c>PUT</c>. Hits
///     <c>packages/conan/v1/files/:package_name/:package_version/:package_username/:package_channel/:recipe_revision/export/:file_name</c>
///     ,
///     the same route <c>DownloadRecipeFileAsync</c> reads.
/// </summary>
internal partial interface IPackagesConanRepository
{
    Task UploadRecipeFileAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default);
}