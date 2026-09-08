using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Repositories;

/// <summary>
///     Part F: uploading the recipe and package file bytes themselves (v1, project-scoped) - the two
///     multipart <c>PUT</c> endpoints GitLab answers with an empty body. Everything else about these two
///     file families (download, Workhorse authorize, and the route helpers this partial reuses) lives in
///     <c>PackagesConanRepository.Files.cs</c>.
/// </summary>
internal partial interface IPackagesConanRepository
{
    Task UploadRecipeFileForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        GitLabFileUpload file, CancellationToken cancellationToken = default);

    Task UploadPackageFileForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default);
}