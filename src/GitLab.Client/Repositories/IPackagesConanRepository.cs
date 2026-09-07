using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Conan package registry (<c>/packages/conan/v1</c>,
///     <c>/projects/:id/packages/conan/v1</c> and <c>/projects/:id/packages/conan/v2</c>): builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls <see cref="IGitLabApiConnection" />.
///     Knows GitLab's wire format; nothing above this layer should build a route or touch
///     <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         GitLab exposes this API at three scopes: instance-wide (no project in the route - a recipe is
///         addressed purely by its coordinates), per-project (<c>v1</c>), and per-project revision-aware
///         (<c>v2</c>, which layers Conan recipe/package revisions on top of the same coordinates and has no
///         instance-wide form). Method names follow that split: no suffix for instance-wide v1, "ForProject"
///         for project-scoped v1, and "ForProjectV2" for v2.
///     </para>
///     <para>
///         Every recipe/package coordinate (<c>package_name</c>, <c>package_version</c>,
///         <c>package_username</c>, <c>package_channel</c>, <c>conan_package_reference</c>,
///         <c>recipe_revision</c>, <c>package_revision</c>, <c>file_name</c>) is caller-supplied free text -
///         a username commonly contains <c>+</c> (a namespaced project path, e.g. <c>my-group+my-project</c>)
///         and a channel or reference can legally contain <c>/</c> and <c>@</c> - so every one goes through
///         <c>.Escaped(...)</c>, never <c>.Literal(...)</c>.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IPackagesConanService), typeof(IPackagesConanClient))]
internal interface IPackagesConanRepository
{
    // ----- Recipes & packages (v1) -----

    Task<JsonElement> SearchAsync(string q, bool? ignoreCase = null, CancellationToken cancellationToken = default);

    Task<JsonElement> SearchForProjectAsync(ProjectId projectId, string q, bool? ignoreCase = null,
        CancellationToken cancellationToken = default);

    Task DeleteRecipeAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, CancellationToken cancellationToken = default);

    Task DeleteRecipeForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default);

    Task<GitLabConanRecipeSnapshot> GetRecipeSnapshotAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default);

    Task<GitLabConanRecipeSnapshot> GetRecipeSnapshotForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    Task<GitLabConanRecipeUrls> GetRecipeManifestAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default);

    Task<GitLabConanRecipeUrls> GetRecipeManifestForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    Task<GitLabConanRecipeUrls> GetRecipeDownloadUrlsAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default);

    Task<GitLabConanRecipeUrls> GetRecipeDownloadUrlsForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    Task<GitLabConanUploadUrls> GetRecipeUploadUrlsAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default);

    Task<GitLabConanUploadUrls> GetRecipeUploadUrlsForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    Task<GitLabConanPackageSnapshot> GetPackageSnapshotAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    Task<GitLabConanPackageSnapshot> GetPackageSnapshotForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    Task<GitLabConanPackageUrls> GetPackageManifestAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    Task<GitLabConanPackageUrls> GetPackageManifestForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    Task<GitLabConanPackageUrls> GetPackageDownloadUrlsAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    Task<GitLabConanPackageUrls> GetPackageDownloadUrlsForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    Task<GitLabConanUploadUrls> GetPackageUploadUrlsAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    Task<GitLabConanUploadUrls> GetPackageUploadUrlsForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    Task<JsonElement> SearchPackageReferencesAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default);

    Task<JsonElement> SearchPackageReferencesForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    // ----- Files (v1) -----

    Task<GitLabFileResponse> DownloadRecipeFileAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadRecipeFileForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string fileName, CancellationToken cancellationToken = default);

    Task AuthorizeRecipeFileUploadAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default);

    Task AuthorizeRecipeFileUploadForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadPackageFileAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadPackageFileForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, string fileName,
        CancellationToken cancellationToken = default);

    Task AuthorizePackageFileUploadAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string conanPackageReference, string packageRevision,
        string fileName, CancellationToken cancellationToken = default);

    Task AuthorizePackageFileUploadForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, CancellationToken cancellationToken = default);

    // ----- Instance & credentials (v1) -----

    Task PingAsync(CancellationToken cancellationToken = default);

    Task PingForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> AuthenticateAsync(CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> AuthenticateForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> CheckCredentialsAsync(CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> CheckCredentialsForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    // ----- Revisions (v2, project-scoped only) -----

    Task<JsonElement> SearchForProjectV2Async(ProjectId projectId, string q, bool? ignoreCase = null,
        CancellationToken cancellationToken = default);

    Task<GitLabConanRevisionInfo> GetLatestRecipeRevisionAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    Task<GitLabConanRecipeRevisions> GetRecipeRevisionsAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    Task DeleteRecipeRevisionAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision,
        CancellationToken cancellationToken = default);

    Task<GitLabConanFileList> GetRecipeRevisionFilesAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadRecipeRevisionFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string fileName, CancellationToken cancellationToken = default);

    Task AuthorizeRecipeRevisionFileUploadAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default);

    Task<GitLabConanRevisionInfo> GetLatestPackageRevisionAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, CancellationToken cancellationToken = default);

    Task<GitLabConanPackageRevisions> GetPackageRevisionsAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, CancellationToken cancellationToken = default);

    Task DeletePackageRevisionAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, CancellationToken cancellationToken = default);

    Task<GitLabConanFileList> GetPackageRevisionFilesAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadPackageRevisionFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, string fileName,
        CancellationToken cancellationToken = default);

    Task AuthorizePackageRevisionFileUploadAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, CancellationToken cancellationToken = default);

    Task<JsonElement> SearchPackageReferencesByRecipeRevisionAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        CancellationToken cancellationToken = default);

    Task<JsonElement> SearchPackageReferencesForProjectV2Async(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> AuthenticateForProjectV2Async(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> CheckCredentialsForProjectV2Async(ProjectId projectId,
        CancellationToken cancellationToken = default);
}