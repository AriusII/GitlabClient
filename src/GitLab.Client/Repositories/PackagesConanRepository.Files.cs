using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;

namespace GitLab.Client.Repositories;

/// <summary>
///     Recipe and package file transfer (v1): raw binary downloads and Workhorse upload-authorization
///     checks, at both instance-wide and per-project scope.
///     <para>
///         GitLab's <c>PUT .../export/:file_name</c> and <c>PUT .../package/.../:file_name</c> endpoints
///         (uploading the recipe/package file itself) are deliberately NOT implemented here: they take a
///         <c>multipart/form-data</c> body and answer <c>200</c> with an empty body, which the
///         JSON-deserializing <c>IGitLabApiConnection.PutFileAsync&lt;TResponse&gt;</c> overload cannot
///         express. The transport has since grown a no-content sibling of <c>PutFileAsync</c> for exactly
///         this shape, and the four upload methods are implemented against it in
///         <c>PackagesConanRepository.B.cs</c> (instance-wide recipe), <c>PackagesConanRepository.C.cs</c>
///         (instance-wide package) and <c>PackagesConanRepository.F.cs</c> (project-scoped recipe and
///         package) - the route helpers declared below are shared with those partials.
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