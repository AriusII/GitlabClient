using GitLab.Client.Abstractions;

namespace GitLab.Client.Services;

/// <summary>Part B: mirrors <c>IPackagesConanRepository.B.cs</c>'s recipe file upload.</summary>
internal partial interface IPackagesConanService
{
    Task UploadRecipeFileAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default);
}