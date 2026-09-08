using GitLab.Client.Domain;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Conan v2 protocol addition, part H: uploading a recipe file to a specific recipe revision - the
///     multipart <c>PUT</c> counterpart to <c>AuthorizeRecipeRevisionFileUploadAsync</c>. This could not
///     previously be expressed because the endpoint answers with an empty body; it now uses the
///     no-content multipart <c>PUT</c> the transport added this round.
/// </summary>
public partial interface IPackagesConanClient
{
    /// <summary>
    ///     Uploads one recipe file (<c>conanfile.py</c>, <c>conanmanifest.txt</c>, <c>conan_sources.tgz</c>
    ///     or <c>conan_export.tgz</c>) to a specific recipe revision. Call
    ///     <see cref="AuthorizeRecipeRevisionFileUploadAsync" /> first, as GitLab's own Conan client does.
    /// </summary>
    Task UploadRecipeRevisionFileAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        GitLabFileUpload file, CancellationToken cancellationToken = default);
}