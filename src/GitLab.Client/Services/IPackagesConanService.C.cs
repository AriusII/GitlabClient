using GitLab.Client.Abstractions;

namespace GitLab.Client.Services;

/// <summary>Part C: mirrors <c>IPackagesConanRepository.C.cs</c> - the instance-wide package file upload.</summary>
internal partial interface IPackagesConanService
{
    Task UploadPackageFileAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string conanPackageReference, string packageRevision,
        string fileName, GitLabFileUpload file, CancellationToken cancellationToken = default);
}