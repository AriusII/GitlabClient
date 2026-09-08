using GitLab.Client.Abstractions;

namespace GitLab.Client.Repositories;

/// <summary>
///     Part B: the instance-wide Conan recipe file upload. Split into its own file because it lands after
///     the rest of this resource and depends on a transport overload (<c>IGitLabApiConnection.PutFileAsync</c>
///     with no <c>TResponse</c>) that did not exist yet when <c>PackagesConanRepository.Files.cs</c> was
///     written - see the remark there.
/// </summary>
internal sealed partial class PackagesConanRepository
{
    public Task UploadRecipeFileAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PutFileAsync(
            RecipeFileRoute(packageName, packageVersion, packageUsername, packageChannel, recipeRevision, fileName)
                .Build(),
            file,
            null,
            cancellationToken);
    }
}