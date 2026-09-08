using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Repositories;

/// <summary>
///     Part F: uploading the recipe and package file bytes themselves (v1, project-scoped).
///     <para>
///         <c>PackagesConanRepository.Files.cs</c> originally left these two endpoints unimplemented
///         because the only multipart <c>PUT</c> the transport exposed at the time,
///         <see cref="IGitLabApiConnection.PutFileAsync{TResponse}" />, requires a JSON response body to
///         deserialize and GitLab answers both of these with an empty <c>200</c>. The transport has since
///         grown a no-content sibling (four parameters, no <c>JsonTypeInfo</c>, plain <c>Task</c>), which
///         is what this partial uses. The route helpers (<c>RecipeFileRoute</c>, <c>PackageFileRoute</c>) and the
///         segment constants are already declared on the main partial class and reused here as-is.
///     </para>
/// </summary>
internal sealed partial class PackagesConanRepository
{
    public Task UploadRecipeFileForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        GitLabFileUpload file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PutFileAsync(
            RecipeFileRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision, fileName).Build(),
            file,
            null,
            cancellationToken);
    }

    public Task UploadPackageFileForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PutFileAsync(
            PackageFileRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision, conanPackageReference, packageRevision, fileName).Build(),
            file,
            null,
            cancellationToken);
    }
}