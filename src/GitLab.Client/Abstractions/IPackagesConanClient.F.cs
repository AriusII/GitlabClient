using GitLab.Client.Domain;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Part F: uploading the recipe and package file bytes themselves (v1, project-scoped) - the two
///     multipart <c>PUT</c> endpoints a Conan client calls after asking the corresponding upload-URLs
///     endpoint (<see cref="IPackagesConanClient.GetRecipeUploadUrlsForProjectAsync" /> /
///     <see cref="IPackagesConanClient.GetPackageUploadUrlsForProjectAsync" />) and confirming the
///     Workhorse authorize check. GitLab answers both with an empty body on success.
/// </summary>
public partial interface IPackagesConanClient
{
    /// <summary>
    ///     Uploads one recipe file (<c>conanfile.py</c>, <c>conanmanifest.txt</c>, <c>conan_export.tgz</c>
    ///     and friends) to a project's Conan registry. Call this only after
    ///     <see cref="GetRecipeUploadUrlsForProjectAsync" /> has returned the upload URL and the matching
    ///     Workhorse authorize check has succeeded.
    /// </summary>
    Task UploadRecipeFileForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        GitLabFileUpload file, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads one package binary file (<c>conaninfo.txt</c>, <c>conanmanifest.txt</c>,
    ///     <c>conan_package.tgz</c> and friends) to a project's Conan registry. Call this only after
    ///     <see cref="GetPackageUploadUrlsForProjectAsync" /> has returned the upload URL and the matching
    ///     Workhorse authorize check has succeeded.
    /// </summary>
    Task UploadPackageFileForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default);
}