using GitLab.Client.Abstractions;

namespace GitLab.Client.Repositories;

/// <summary>
///     Part C: the instance-wide Conan package file upload
///     (<c>PUT packages/conan/v1/files/.../package/.../:file_name</c>). GitLab answers this Workhorse
///     upload with a bare success status and an empty body, which is exactly what
///     <see
///         cref="IGitLabApiConnection.PutFileAsync(Uri, GitLabFileUpload, IReadOnlyDictionary{string, string}, CancellationToken)" />
///     (the no-content multipart <c>PUT</c> overload) exists for - see that overload's remarks, which
///     name this exact endpoint.
/// </summary>
internal sealed partial class PackagesConanRepository
{
    public Task UploadPackageFileAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string conanPackageReference, string packageRevision,
        string fileName, GitLabFileUpload file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PutFileAsync(
            PackageFileRoute(packageName, packageVersion, packageUsername, packageChannel, recipeRevision,
                conanPackageReference, packageRevision, fileName).Build(),
            file,
            null,
            cancellationToken);
    }
}