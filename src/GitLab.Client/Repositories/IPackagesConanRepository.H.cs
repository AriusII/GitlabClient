using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Repositories;

/// <summary>
///     Conan v2 protocol addition, part H: uploading a recipe file to a specific recipe revision. This
///     is the one <c>PUT .../revisions/:recipe_revision/files/:file_name</c> endpoint that
///     <c>PackagesConanRepository.V2.cs</c> could not previously implement, because it answers with an
///     empty body and the transport only had a JSON-deserializing multipart <c>PUT</c>. See
///     <c>PackagesConanRepository.H.cs</c> for the implementation, which uses the no-content
///     <c>IGitLabApiConnection.PutFileAsync</c> overload added this round.
/// </summary>
internal partial interface IPackagesConanRepository
{
    Task UploadRecipeRevisionFileAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        GitLabFileUpload file, CancellationToken cancellationToken = default);
}