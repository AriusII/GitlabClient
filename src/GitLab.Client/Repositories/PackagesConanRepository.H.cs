using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Repositories;

/// <summary>
///     Conan v2 protocol addition, part H: the recipe-revision file upload that
///     <c>PackagesConanRepository.V2.cs</c> could not previously implement. GitLab's
///     <c>PUT .../revisions/:recipe_revision/files/:file_name</c> endpoint is a multipart upload that
///     answers with an empty body, which needs the no-content
///     <c>
///         IGitLabApiConnection.PutFileAsync(Uri, GitLabFileUpload, IReadOnlyDictionary&lt;string, string&gt;?,
///         CancellationToken)
///     </c>
///     overload rather than the generic, JSON-deserializing one.
/// </summary>
internal sealed partial class PackagesConanRepository
{
    public Task UploadRecipeRevisionFileAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        GitLabFileUpload file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PutFileAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision).Literal(FilesSegment).Escaped(fileName).Build(),
            file,
            null,
            cancellationToken);
    }
}