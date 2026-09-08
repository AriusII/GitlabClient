using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Services;

/// <summary>
///     Conan v2 protocol addition, part H: mirrors <c>IPackagesConanRepository.H.cs</c> - see there for
///     why this one recipe-revision file upload needed its own partial file.
/// </summary>
internal partial interface IPackagesConanService
{
    Task UploadRecipeRevisionFileAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        GitLabFileUpload file, CancellationToken cancellationToken = default);
}