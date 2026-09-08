using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     The Conan v2 protocol area: revision-aware recipe and package metadata, files and file transfer,
///     project-scoped only - GitLab exposes no instance-wide v2 route.
///     <para>
///         As in <c>PackagesConanRepository.Files.cs</c>, the two <c>PUT .../files/:file_name</c> upload
///         endpoints (recipe-revision and package-revision file upload) could not originally be expressed
///         here: they are a multipart <c>PUT</c> that answers with an empty body, and
///         <c>IGitLabApiConnection.PutFileAsync&lt;TResponse&gt;</c> requires a JSON response to
///         deserialize. Both are now implemented against the transport's no-content <c>PutFileAsync</c>
///         sibling - the recipe-revision upload in <c>PackagesConanRepository.H.cs</c> and the
///         package-revision upload in <c>PackagesConanRepository.I.cs</c> - reusing the route helpers
///         declared below.
///     </para>
/// </summary>
internal sealed partial class PackagesConanRepository
{
    private const string RevisionsSegment = "revisions";
    private const string LatestSegment = "latest";
    private const string SearchSegment = "search";

    public Task<JsonElement> SearchForProjectV2Async(ProjectId projectId, string q, bool? ignoreCase = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectV2Route(projectId).Literal(ConansSegment).Literal(SearchSegment)
                .Query("q", q)
                .Query("ignorecase", ignoreCase)
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<GitLabConanRevisionInfo> GetLatestRecipeRevisionAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRouteV2(projectId, packageName, packageVersion, packageUsername, packageChannel)
                .Literal(LatestSegment).Build(),
            GitLabJsonContext.Default.GitLabConanRevisionInfo,
            cancellationToken);
    }

    public Task<GitLabConanRecipeRevisions> GetRecipeRevisionsAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRouteV2(projectId, packageName, packageVersion, packageUsername, packageChannel)
                .Literal(RevisionsSegment).Build(),
            GitLabJsonContext.Default.GitLabConanRecipeRevisions,
            cancellationToken);
    }

    public Task DeleteRecipeRevisionAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision).Build(),
            cancellationToken);
    }

    public Task<GitLabConanFileList> GetRecipeRevisionFilesAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision).Literal(FilesSegment).Build(),
            GitLabJsonContext.Default.GitLabConanFileList,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadRecipeRevisionFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string fileName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision).Literal(FilesSegment).Escaped(fileName).Build(),
            cancellationToken);
    }

    public Task AuthorizeRecipeRevisionFileUploadAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string fileName, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision).Literal(FilesSegment).Escaped(fileName).Literal(AuthorizeSegment).Build(),
            cancellationToken);
    }

    public Task<GitLabConanRevisionInfo> GetLatestPackageRevisionAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                    recipeRevision)
                .Literal(PackagesSegment).Escaped(conanPackageReference).Literal(LatestSegment).Build(),
            GitLabJsonContext.Default.GitLabConanRevisionInfo,
            cancellationToken);
    }

    public Task<GitLabConanPackageRevisions> GetPackageRevisionsAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                    recipeRevision)
                .Literal(PackagesSegment).Escaped(conanPackageReference).Literal(RevisionsSegment).Build(),
            GitLabJsonContext.Default.GitLabConanPackageRevisions,
            cancellationToken);
    }

    public Task DeletePackageRevisionAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            PackageRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision, conanPackageReference, packageRevision).Build(),
            cancellationToken);
    }

    public Task<GitLabConanFileList> GetPackageRevisionFilesAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            PackageRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision, conanPackageReference, packageRevision).Literal(FilesSegment).Build(),
            GitLabJsonContext.Default.GitLabConanFileList,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadPackageRevisionFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            PackageRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                    recipeRevision, conanPackageReference, packageRevision).Literal(FilesSegment).Escaped(fileName)
                .Build(),
            cancellationToken);
    }

    public Task AuthorizePackageRevisionFileUploadAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            PackageRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                    recipeRevision, conanPackageReference, packageRevision)
                .Literal(FilesSegment).Escaped(fileName).Literal(AuthorizeSegment).Build(),
            cancellationToken);
    }

    public Task<JsonElement> SearchPackageReferencesByRecipeRevisionAsync(ProjectId projectId,
        string packageName, string packageVersion, string packageUsername, string packageChannel,
        string recipeRevision, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision).Literal(SearchSegment).Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> SearchPackageReferencesForProjectV2Async(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRouteV2(projectId, packageName, packageVersion, packageUsername, packageChannel)
                .Literal(SearchSegment).Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<GitLabFileResponse> AuthenticateForProjectV2Async(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectV2Route(projectId).Literal(UsersSegment).Literal("authenticate").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> CheckCredentialsForProjectV2Async(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectV2Route(projectId).Literal(UsersSegment).Literal("check_credentials").Build(),
            cancellationToken);
    }

    /// <summary>
    ///     <c>projects/:id/packages/conan/v2/conans/:package_name/:package_version/:package_username/:package_channel</c>.
    /// </summary>
    private static GitLabRouteBuilder RecipeRouteV2(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel)
    {
        return ProjectV2Route(projectId).Literal(ConansSegment)
            .Escaped(packageName).Escaped(packageVersion).Escaped(packageUsername).Escaped(packageChannel);
    }

    /// <summary>Appends <c>revisions/:recipe_revision</c> to a v2 recipe route.</summary>
    private static GitLabRouteBuilder RecipeRevisionRoute(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision)
    {
        return RecipeRouteV2(projectId, packageName, packageVersion, packageUsername, packageChannel)
            .Literal(RevisionsSegment).Escaped(recipeRevision);
    }

    /// <summary>Appends <c>packages/:conan_package_reference/revisions/:package_revision</c> to a recipe-revision route.</summary>
    private static GitLabRouteBuilder PackageRevisionRoute(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision)
    {
        return RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision)
            .Literal(PackagesSegment).Escaped(conanPackageReference)
            .Literal(RevisionsSegment).Escaped(packageRevision);
    }
}