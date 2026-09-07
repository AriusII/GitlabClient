using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the Conan package registry, sitting between the public
///     <c>IPackagesConanClient</c> controller and <c>IPackagesConanRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IPackagesConanService
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