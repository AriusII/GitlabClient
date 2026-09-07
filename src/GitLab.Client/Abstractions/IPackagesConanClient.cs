using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's Conan package registry API - the endpoints a Conan client talks to when its remote
///     points at GitLab (<c>/packages/conan/v1</c> instance-wide, <c>/projects/:id/packages/conan/v1</c>
///     and the revision-aware <c>/projects/:id/packages/conan/v2</c>).
///     <para>
///         Conan mixes two shapes of endpoint: small JSON documents (search results, recipe/package
///         metadata, revision listings) and raw binary transfers (<c>conanfile.py</c>,
///         <c>conanmanifest.txt</c>, <c>conan_export.tgz</c>, <c>conan_package.tgz</c> and friends). The
///         JSON side comes back as typed DTOs where GitLab documents a shape, or as a raw
///         <see cref="JsonElement" /> where it does not; the binary side always comes back as a
///         <see cref="GitLabFileResponse" /> that the caller must dispose.
///     </para>
///     <para>
///         GitLab exposes this API at three scopes. Instance-wide (no project in the route - GitLab
///         resolves the project from the recipe's <c>package_username</c> coordinate) and per-project are
///         both Conan v1; per-project v2 additionally exposes recipe and package revisions and has no
///         instance-wide form. Method names follow that split: no suffix for instance-wide v1,
///         <c>ForProject</c> for project-scoped v1, and <c>ForProjectV2</c> for v2.
///     </para>
///     <para>
///         A recipe is addressed by four coordinates throughout this client: <c>packageName</c>,
///         <c>packageVersion</c>, <c>packageUsername</c> and <c>packageChannel</c> - together the Conan
///         reference <c>name/version@user/channel</c>. A package binary additionally needs a
///         <c>conanPackageReference</c> (the package ID hash); v2 additionally threads a
///         <c>recipeRevision</c> and, for packages, a <c>packageRevision</c>. Every one of these is free
///         text a caller supplies and is percent-encoded by this client - pass the raw value.
///     </para>
/// </summary>
public interface IPackagesConanClient
{
    // ----- Recipes & packages (v1) -----

    /// <summary>
    ///     Searches the instance for a Conan package by name. GitLab does not document a response schema
    ///     for this endpoint, so the result is a raw <see cref="JsonElement" />.
    /// </summary>
    Task<JsonElement> SearchAsync(string q, bool? ignoreCase = null, CancellationToken cancellationToken = default);

    /// <summary>Searches one project's Conan package registry by name.</summary>
    Task<JsonElement> SearchForProjectAsync(ProjectId projectId, string q, bool? ignoreCase = null,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a Conan recipe and every package built from it.</summary>
    Task DeleteRecipeAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, CancellationToken cancellationToken = default);

    /// <summary>Deletes a Conan recipe and every package built from it, from one project's registry.</summary>
    Task DeleteRecipeForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default);

    /// <summary>Gets the recipe file snapshot - every recipe file GitLab holds, mapped to its checksum.</summary>
    Task<GitLabConanRecipeSnapshot> GetRecipeSnapshotAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default);

    /// <summary>Gets the recipe file snapshot from one project's registry.</summary>
    Task<GitLabConanRecipeSnapshot> GetRecipeSnapshotForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the recipe manifest - the recipe files mapped to the URL of their checksum digest.</summary>
    Task<GitLabConanRecipeUrls> GetRecipeManifestAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default);

    /// <summary>Gets the recipe manifest from one project's registry.</summary>
    Task<GitLabConanRecipeUrls> GetRecipeManifestForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the download URL for every file in the recipe.</summary>
    Task<GitLabConanRecipeUrls> GetRecipeDownloadUrlsAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default);

    /// <summary>Gets the download URL for every file in the recipe, from one project's registry.</summary>
    Task<GitLabConanRecipeUrls> GetRecipeDownloadUrlsForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Requests the upload URL for every recipe file about to be pushed. Conan calls this before
    ///     uploading the recipe itself.
    /// </summary>
    Task<GitLabConanUploadUrls> GetRecipeUploadUrlsAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default);

    /// <summary>Requests the recipe upload URLs from one project's registry.</summary>
    Task<GitLabConanUploadUrls> GetRecipeUploadUrlsForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the package file snapshot - every binary file GitLab holds for one package reference.</summary>
    Task<GitLabConanPackageSnapshot> GetPackageSnapshotAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the package file snapshot from one project's registry.</summary>
    Task<GitLabConanPackageSnapshot> GetPackageSnapshotForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the package manifest - the package files mapped to the URL of their checksum digest.</summary>
    Task<GitLabConanPackageUrls> GetPackageManifestAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the package manifest from one project's registry.</summary>
    Task<GitLabConanPackageUrls> GetPackageManifestForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the download URL for every file in the package.</summary>
    Task<GitLabConanPackageUrls> GetPackageDownloadUrlsAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the download URL for every file in the package, from one project's registry.</summary>
    Task<GitLabConanPackageUrls> GetPackageDownloadUrlsForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Requests the upload URL for every package file about to be pushed. Conan calls this before
    ///     uploading a package binary.
    /// </summary>
    Task<GitLabConanUploadUrls> GetPackageUploadUrlsAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    /// <summary>Requests the package upload URLs from one project's registry.</summary>
    Task<GitLabConanUploadUrls> GetPackageUploadUrlsForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the metadata for every package reference built from a recipe. GitLab does not document a
    ///     response schema for this endpoint, so the result is a raw <see cref="JsonElement" />.
    /// </summary>
    Task<JsonElement> SearchPackageReferencesAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default);

    /// <summary>Gets the package-reference metadata from one project's registry.</summary>
    Task<JsonElement> SearchPackageReferencesForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    // ----- Files (v1) -----

    /// <summary>
    ///     Downloads one recipe file (<c>conanfile.py</c>, <c>conanmanifest.txt</c>, <c>conan_sources.tgz</c>
    ///     or <c>conan_export.tgz</c>). Use the URL from <see cref="GetRecipeDownloadUrlsAsync" /> rather
    ///     than guessing the revision, as GitLab's own documentation directs.
    /// </summary>
    Task<GitLabFileResponse> DownloadRecipeFileAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads one recipe file from one project's registry.</summary>
    Task<GitLabFileResponse> DownloadRecipeFileForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     The Workhorse pre-upload authorization check for a recipe file - Conan (and this client) must
    ///     call this before <c>PUT</c>-ing the file itself. There is currently no way to express the upload
    ///     itself through this client; see the resource's implementation notes.
    /// </summary>
    Task AuthorizeRecipeFileUploadAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>The recipe file upload authorization check against one project's registry.</summary>
    Task AuthorizeRecipeFileUploadForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one package binary file (<c>conaninfo.txt</c>, <c>conanmanifest.txt</c> or
    ///     <c>conan_package.tgz</c>). Use the URL from <see cref="GetPackageDownloadUrlsAsync" /> rather
    ///     than guessing the revision.
    /// </summary>
    Task<GitLabFileResponse> DownloadPackageFileAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, CancellationToken cancellationToken = default);

    /// <summary>Downloads one package binary file from one project's registry.</summary>
    Task<GitLabFileResponse> DownloadPackageFileForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>The Workhorse pre-upload authorization check for a package file.</summary>
    Task AuthorizePackageFileUploadAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string conanPackageReference, string packageRevision,
        string fileName, CancellationToken cancellationToken = default);

    /// <summary>The package file upload authorization check against one project's registry.</summary>
    Task AuthorizePackageFileUploadForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, CancellationToken cancellationToken = default);

    // ----- Instance & credentials (v1) -----

    /// <summary>Checks that the instance-wide Conan repository is reachable. The answer is the status code alone.</summary>
    Task PingAsync(CancellationToken cancellationToken = default);

    /// <summary>Checks that a project's Conan repository is reachable. The answer is the status code alone.</summary>
    Task PingForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a JSON Web Token for use as the Conan client's Bearer credential on subsequent
    ///     requests. GitLab does not document the body's content type, so it is returned unread as a
    ///     <see cref="GitLabFileResponse" /> - read <see cref="GitLabFileResponse.Content" /> as text to get
    ///     the token.
    /// </summary>
    Task<GitLabFileResponse> AuthenticateAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves a Bearer token scoped to one project's Conan registry.</summary>
    Task<GitLabFileResponse> AuthenticateForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Verifies that the credentials on the request are valid for the instance-wide Conan registry.
    /// </summary>
    Task<GitLabFileResponse> CheckCredentialsAsync(CancellationToken cancellationToken = default);

    /// <summary>Verifies that the credentials on the request are valid for one project's Conan registry.</summary>
    Task<GitLabFileResponse> CheckCredentialsForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    // ----- Revisions (v2, project-scoped only) -----

    /// <summary>Searches one project's Conan package registry by name, through the Conan v2 protocol route.</summary>
    Task<JsonElement> SearchForProjectV2Async(ProjectId projectId, string q, bool? ignoreCase = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the current (most recent) revision hash of a recipe.</summary>
    Task<GitLabConanRevisionInfo> GetLatestRecipeRevisionAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    /// <summary>Lists every revision GitLab has recorded for a recipe, newest first.</summary>
    Task<GitLabConanRecipeRevisions> GetRecipeRevisionsAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one recipe revision and everything built against it.</summary>
    Task DeleteRecipeRevisionAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the files GitLab holds for one recipe revision.</summary>
    Task<GitLabConanFileList> GetRecipeRevisionFilesAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads one file from a specific recipe revision.</summary>
    Task<GitLabFileResponse> DownloadRecipeRevisionFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string fileName, CancellationToken cancellationToken = default);

    /// <summary>The Workhorse pre-upload authorization check for a file on a specific recipe revision.</summary>
    Task AuthorizeRecipeRevisionFileUploadAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the current (most recent) revision hash of a package reference, at a given recipe revision.</summary>
    Task<GitLabConanRevisionInfo> GetLatestPackageRevisionAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, CancellationToken cancellationToken = default);

    /// <summary>Lists every revision GitLab has recorded for a package reference, newest first.</summary>
    Task<GitLabConanPackageRevisions> GetPackageRevisionsAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, CancellationToken cancellationToken = default);

    /// <summary>Deletes one package revision.</summary>
    Task DeletePackageRevisionAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, CancellationToken cancellationToken = default);

    /// <summary>Lists the files GitLab holds for one package revision.</summary>
    Task<GitLabConanFileList> GetPackageRevisionFilesAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, CancellationToken cancellationToken = default);

    /// <summary>Downloads one file from a specific package revision.</summary>
    Task<GitLabFileResponse> DownloadPackageRevisionFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>The Workhorse pre-upload authorization check for a file on a specific package revision.</summary>
    Task AuthorizePackageRevisionFileUploadAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the metadata for every package reference built from a recipe, pinned to one recipe revision.
    ///     GitLab does not document a response schema for this endpoint, so the result is a raw
    ///     <see cref="JsonElement" />.
    /// </summary>
    Task<JsonElement> SearchPackageReferencesByRecipeRevisionAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the metadata for every package reference built from a recipe, through the Conan v2 protocol
    ///     route (no pinned recipe revision).
    /// </summary>
    Task<JsonElement> SearchPackageReferencesForProjectV2Async(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a Bearer token for one project's Conan registry, through the Conan v2 protocol route.</summary>
    Task<GitLabFileResponse> AuthenticateForProjectV2Async(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Verifies credentials against one project's Conan registry, through the Conan v2 protocol route.</summary>
    Task<GitLabFileResponse> CheckCredentialsForProjectV2Async(ProjectId projectId,
        CancellationToken cancellationToken = default);
}