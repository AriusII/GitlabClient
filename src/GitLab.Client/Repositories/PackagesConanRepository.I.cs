using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Repositories;

/// <summary>
///     Conan v2 package-revision file upload - one slice of the "Packages: Conan" tag's 10-way scope
///     split. This is the actual
///     <c>PUT .../packages/:conan_package_reference/revisions/:package_revision/files/:file_name</c>
///     upload, which <c>PackagesConanRepository.V2.cs</c> previously documented as unexpressible because
///     the only multipart <c>PUT</c> overload the transport exposed required a JSON response to
///     deserialize, and GitLab's Workhorse layer answers this endpoint with an empty body. It is
///     expressible now that <see cref="IGitLabApiConnection" /> has grown a no-content
///     <c>PutFileAsync(Uri, GitLabFileUpload, IReadOnlyDictionary{string, string}?, CancellationToken)</c>
///     sibling. The analogous recipe-revision file upload
///     (<c>PUT .../conans/.../revisions/:recipe_revision/files/:file_name</c>) and the two v1 uploads
///     documented in <c>PackagesConanRepository.Files.cs</c> are out of this slice's scope; whichever
///     slice owns those routes should wire them up the same way.
/// </summary>
internal sealed partial class PackagesConanRepository
{
    public Task UploadPackageRevisionFileAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PutFileAsync(
            PackageRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                    recipeRevision, conanPackageReference, packageRevision)
                .Literal(FilesSegment).Escaped(fileName).Build(),
            file,
            null,
            cancellationToken);
    }
}