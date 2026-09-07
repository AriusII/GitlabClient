using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;

namespace GitLab.Client.Repositories;

/// <summary>
///     Recipe and package file transfer (v1): raw binary downloads and Workhorse upload-authorization
///     checks, at both instance-wide and per-project scope.
///     <para>
///         GitLab's <c>PUT .../export/:file_name</c> and <c>PUT .../package/.../:file_name</c> endpoints
///         (uploading the recipe/package file itself) are deliberately NOT implemented here. Both take a
///         <c>multipart/form-data</c> body and answer <c>200</c> with an empty body, but
///         <c>IGitLabApiConnection.PutFileAsync&lt;TResponse&gt;</c> is the only multipart <c>PUT</c> the
///         transport exposes and it requires a JSON response to deserialize - calling it against a
///         genuinely empty body throws. There is no no-content sibling of <c>PutFileAsync</c> (unlike
///         <c>PostFileAsync</c>, which has one). See this resource's implementation report for the six
///         operations this affects.
///     </para>
/// </summary>
internal sealed partial class PackagesConanRepository
{
    private const string FilesSegment = "files";
    private const string ExportSegment = "export";
    private const string PackageSegment = "package";
    private const string AuthorizeSegment = "authorize";

    public Task<GitLabFileResponse> DownloadRecipeFileAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            RecipeFileRoute(packageName, packageVersion, packageUsername, packageChannel, recipeRevision, fileName)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadRecipeFileForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string fileName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            RecipeFileRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision, fileName).Build(),
            cancellationToken);
    }

    public Task AuthorizeRecipeFileUploadAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            RecipeFileRoute(packageName, packageVersion, packageUsername, packageChannel, recipeRevision, fileName)
                .Literal(AuthorizeSegment).Build(),
            cancellationToken);
    }

    public Task AuthorizeRecipeFileUploadForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string fileName, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            RecipeFileRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision, fileName).Literal(AuthorizeSegment).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadPackageFileAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            PackageFileRoute(packageName, packageVersion, packageUsername, packageChannel, recipeRevision,
                conanPackageReference, packageRevision, fileName).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadPackageFileForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            PackageFileRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision, conanPackageReference, packageRevision, fileName).Build(),
            cancellationToken);
    }

    public Task AuthorizePackageFileUploadAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string conanPackageReference, string packageRevision,
        string fileName, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            PackageFileRoute(packageName, packageVersion, packageUsername, packageChannel, recipeRevision,
                conanPackageReference, packageRevision, fileName).Literal(AuthorizeSegment).Build(),
            cancellationToken);
    }

    public Task AuthorizePackageFileUploadForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            PackageFileRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                    recipeRevision, conanPackageReference, packageRevision, fileName).Literal(AuthorizeSegment)
                .Build(),
            cancellationToken);
    }

    /// <summary>
    ///     <c>packages/conan/v1/files/:package_name/:package_version/:package_username/:package_channel/:recipe_revision/export/:file_name</c>
    ///     .
    /// </summary>
    private static GitLabRouteBuilder RecipeFileRoute(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName)
    {
        return InstanceRoute().Literal(FilesSegment)
            .Escaped(packageName).Escaped(packageVersion).Escaped(packageUsername).Escaped(packageChannel)
            .Escaped(recipeRevision).Literal(ExportSegment).Escaped(fileName);
    }

    /// <summary>
    ///     <c>projects/:id/packages/conan/v1/files/:package_name/:package_version/:package_username/:package_channel/:recipe_revision/export/:file_name</c>
    ///     .
    /// </summary>
    private static GitLabRouteBuilder RecipeFileRoute(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string fileName)
    {
        return ProjectRoute(projectId).Literal(FilesSegment)
            .Escaped(packageName).Escaped(packageVersion).Escaped(packageUsername).Escaped(packageChannel)
            .Escaped(recipeRevision).Literal(ExportSegment).Escaped(fileName);
    }

    /// <summary>
    ///     <c>packages/conan/v1/files/.../:recipe_revision/package/:conan_package_reference/:package_revision/:file_name</c>.
    /// </summary>
    private static GitLabRouteBuilder PackageFileRoute(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName)
    {
        return InstanceRoute().Literal(FilesSegment)
            .Escaped(packageName).Escaped(packageVersion).Escaped(packageUsername).Escaped(packageChannel)
            .Escaped(recipeRevision).Literal(PackageSegment).Escaped(conanPackageReference)
            .Escaped(packageRevision).Escaped(fileName);
    }

    /// <summary>
    ///     <c>projects/:id/packages/conan/v1/files/.../:recipe_revision/package/:conan_package_reference/:package_revision/:file_name</c>
    ///     .
    /// </summary>
    private static GitLabRouteBuilder PackageFileRoute(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, string fileName)
    {
        return ProjectRoute(projectId).Literal(FilesSegment)
            .Escaped(packageName).Escaped(packageVersion).Escaped(packageUsername).Escaped(packageChannel)
            .Escaped(recipeRevision).Literal(PackageSegment).Escaped(conanPackageReference)
            .Escaped(packageRevision).Escaped(fileName);
    }
}