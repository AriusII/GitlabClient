using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

/// <summary>
///     Recipe and package metadata (v1): search, snapshots, manifests, download URLs and upload URLs, at
///     both instance-wide and per-project scope. The other regions of this resource - raw file transfer,
///     the instance/credentials checks, and the Conan v2 protocol area - live in this same direct endpoint
///     client.
/// </summary>
internal sealed class PackagesConanClient(IGitLabApiConnection connection) : IPackagesConanClient
{
    private const string PackagesSegment = "packages";
    private const string ConanSegment = "conan";
    private const string V1Segment = "v1";
    private const string ConansSegment = "conans";

    private const string FilesSegment = "files";
    private const string ExportSegment = "export";
    private const string PackageSegment = "package";
    private const string AuthorizeSegment = "authorize";
    private const string UsersSegment = "users";
    private const string RevisionsSegment = "revisions";
    private const string LatestSegment = "latest";
    private const string SearchSegment = "search";

    public Task<JsonElement> SearchAsync(string q, bool? ignoreCase = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            InstanceRoute().Literal(ConansSegment).Literal("search")
                .Query("q", q)
                .Query("ignorecase", ignoreCase)
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> SearchForProjectAsync(ProjectId projectId, string q, bool? ignoreCase = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectRoute(projectId).Literal(ConansSegment).Literal("search")
                .Query("q", q)
                .Query("ignorecase", ignoreCase)
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task DeleteRecipeAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            RecipeRoute(packageName, packageVersion, packageUsername, packageChannel).Build(),
            cancellationToken);
    }

    public Task DeleteRecipeForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            RecipeRoute(projectId, packageName, packageVersion, packageUsername, packageChannel).Build(),
            cancellationToken);
    }

    public Task<GitLabConanRecipeSnapshot> GetRecipeSnapshotAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRoute(packageName, packageVersion, packageUsername, packageChannel).Build(),
            GitLabJsonContext.Default.GitLabConanRecipeSnapshot,
            cancellationToken);
    }

    public Task<GitLabConanRecipeSnapshot> GetRecipeSnapshotForProjectAsync(ProjectId projectId,
        string packageName, string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRoute(projectId, packageName, packageVersion, packageUsername, packageChannel).Build(),
            GitLabJsonContext.Default.GitLabConanRecipeSnapshot,
            cancellationToken);
    }

    public Task<GitLabConanRecipeUrls> GetRecipeManifestAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRoute(packageName, packageVersion, packageUsername, packageChannel).Literal("digest").Build(),
            GitLabJsonContext.Default.GitLabConanRecipeUrls,
            cancellationToken);
    }

    public Task<GitLabConanRecipeUrls> GetRecipeManifestForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRoute(projectId, packageName, packageVersion, packageUsername, packageChannel)
                .Literal("digest").Build(),
            GitLabJsonContext.Default.GitLabConanRecipeUrls,
            cancellationToken);
    }

    public Task<GitLabConanRecipeUrls> GetRecipeDownloadUrlsAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRoute(packageName, packageVersion, packageUsername, packageChannel)
                .Literal("download_urls").Build(),
            GitLabJsonContext.Default.GitLabConanRecipeUrls,
            cancellationToken);
    }

    public Task<GitLabConanRecipeUrls> GetRecipeDownloadUrlsForProjectAsync(ProjectId projectId,
        string packageName, string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRoute(projectId, packageName, packageVersion, packageUsername, packageChannel)
                .Literal("download_urls").Build(),
            GitLabJsonContext.Default.GitLabConanRecipeUrls,
            cancellationToken);
    }

    public Task<GitLabConanUploadUrls> GetRecipeUploadUrlsAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RecipeRoute(packageName, packageVersion, packageUsername, packageChannel)
                .Literal("upload_urls").Build(),
            GitLabJsonContext.Default.GitLabConanUploadUrls,
            cancellationToken);
    }

    public Task<GitLabConanUploadUrls> GetRecipeUploadUrlsForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RecipeRoute(projectId, packageName, packageVersion, packageUsername, packageChannel)
                .Literal("upload_urls").Build(),
            GitLabJsonContext.Default.GitLabConanUploadUrls,
            cancellationToken);
    }

    public Task<GitLabConanPackageSnapshot> GetPackageSnapshotAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            PackageRoute(packageName, packageVersion, packageUsername, packageChannel, conanPackageReference)
                .Build(),
            GitLabJsonContext.Default.GitLabConanPackageSnapshot,
            cancellationToken);
    }

    public Task<GitLabConanPackageSnapshot> GetPackageSnapshotForProjectAsync(ProjectId projectId,
        string packageName, string packageVersion, string packageUsername, string packageChannel,
        string conanPackageReference, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            PackageRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                conanPackageReference).Build(),
            GitLabJsonContext.Default.GitLabConanPackageSnapshot,
            cancellationToken);
    }

    public Task<GitLabConanPackageUrls> GetPackageManifestAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            PackageRoute(packageName, packageVersion, packageUsername, packageChannel, conanPackageReference)
                .Literal("digest").Build(),
            GitLabJsonContext.Default.GitLabConanPackageUrls,
            cancellationToken);
    }

    public Task<GitLabConanPackageUrls> GetPackageManifestForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            PackageRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                conanPackageReference).Literal("digest").Build(),
            GitLabJsonContext.Default.GitLabConanPackageUrls,
            cancellationToken);
    }

    public Task<GitLabConanPackageUrls> GetPackageDownloadUrlsAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            PackageRoute(packageName, packageVersion, packageUsername, packageChannel, conanPackageReference)
                .Literal("download_urls").Build(),
            GitLabJsonContext.Default.GitLabConanPackageUrls,
            cancellationToken);
    }

    public Task<GitLabConanPackageUrls> GetPackageDownloadUrlsForProjectAsync(ProjectId projectId,
        string packageName, string packageVersion, string packageUsername, string packageChannel,
        string conanPackageReference, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            PackageRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                conanPackageReference).Literal("download_urls").Build(),
            GitLabJsonContext.Default.GitLabConanPackageUrls,
            cancellationToken);
    }

    public Task<GitLabConanUploadUrls> GetPackageUploadUrlsAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            PackageRoute(packageName, packageVersion, packageUsername, packageChannel, conanPackageReference)
                .Literal("upload_urls").Build(),
            GitLabJsonContext.Default.GitLabConanUploadUrls,
            cancellationToken);
    }

    public Task<GitLabConanUploadUrls> GetPackageUploadUrlsForProjectAsync(ProjectId projectId,
        string packageName, string packageVersion, string packageUsername, string packageChannel,
        string conanPackageReference, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            PackageRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                conanPackageReference).Literal("upload_urls").Build(),
            GitLabJsonContext.Default.GitLabConanUploadUrls,
            cancellationToken);
    }

    public Task<JsonElement> SearchPackageReferencesAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRoute(packageName, packageVersion, packageUsername, packageChannel).Literal("search").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> SearchPackageReferencesForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRoute(projectId, packageName, packageVersion, packageUsername, packageChannel)
                .Literal("search").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task UploadRecipeFileAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        return connection.PutFileAsync(
            RecipeFileRoute(packageName, packageVersion, packageUsername, packageChannel, recipeRevision, fileName)
                .Build(), file, null, cancellationToken);
    }

    public Task UploadPackageFileAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string conanPackageReference, string packageRevision,
        string fileName, GitLabFileUpload file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        return connection.PutFileAsync(
            PackageFileRoute(packageName, packageVersion, packageUsername, packageChannel, recipeRevision,
                conanPackageReference, packageRevision, fileName).Build(), file, null, cancellationToken);
    }

    public Task UploadRecipeFileForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        return connection.PutFileAsync(
            RecipeFileRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision,
                fileName).Build(), file, null, cancellationToken);
    }

    public Task UploadPackageFileForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, GitLabFileUpload file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        return connection.PutFileAsync(
            PackageFileRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision,
                conanPackageReference, packageRevision, fileName).Build(), file, null, cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadRecipeFileAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            RecipeFileRoute(packageName, packageVersion, packageUsername, packageChannel, recipeRevision, fileName)
                .Build(), cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadRecipeFileForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            RecipeFileRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision,
                fileName).Build(), cancellationToken);
    }

    public Task AuthorizeRecipeFileUploadAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string fileName, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            RecipeFileRoute(packageName, packageVersion, packageUsername, packageChannel, recipeRevision, fileName)
                .Literal(AuthorizeSegment).Build(), cancellationToken);
    }

    public Task AuthorizeRecipeFileUploadForProjectAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            RecipeFileRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision,
                fileName).Literal(AuthorizeSegment).Build(), cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadPackageFileAsync(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            PackageFileRoute(packageName, packageVersion, packageUsername, packageChannel, recipeRevision,
                conanPackageReference, packageRevision, fileName).Build(), cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadPackageFileForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            PackageFileRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision,
                conanPackageReference, packageRevision, fileName).Build(), cancellationToken);
    }

    public Task AuthorizePackageFileUploadAsync(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string conanPackageReference, string packageRevision,
        string fileName, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            PackageFileRoute(packageName, packageVersion, packageUsername, packageChannel, recipeRevision,
                conanPackageReference, packageRevision, fileName).Literal(AuthorizeSegment).Build(), cancellationToken);
    }

    public Task AuthorizePackageFileUploadForProjectAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            PackageFileRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision,
                conanPackageReference, packageRevision, fileName).Literal(AuthorizeSegment).Build(), cancellationToken);
    }

    public Task UploadRecipeRevisionFileAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        return connection.PutFileAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision)
                .Literal(FilesSegment).Escaped(fileName).Build(), file, null, cancellationToken);
    }

    public Task UploadPackageRevisionFileAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, GitLabFileUpload file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        return connection.PutFileAsync(
            PackageRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision, conanPackageReference, packageRevision).Literal(FilesSegment).Escaped(fileName).Build(),
            file, null, cancellationToken);
    }

    public Task PingAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(InstanceRoute().Literal("ping").Build(), cancellationToken);
    }

    public Task PingForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(ProjectRoute(projectId).Literal("ping").Build(), cancellationToken);
    }

    public Task<GitLabFileResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(InstanceRoute().Literal(UsersSegment).Literal("authenticate").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> AuthenticateForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(ProjectRoute(projectId).Literal(UsersSegment).Literal("authenticate").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> CheckCredentialsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(InstanceRoute().Literal(UsersSegment).Literal("check_credentials").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> CheckCredentialsForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectRoute(projectId).Literal(UsersSegment).Literal("check_credentials").Build(), cancellationToken);
    }

    public Task<JsonElement> SearchForProjectV2Async(ProjectId projectId, string q, bool? ignoreCase = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectV2Route(projectId).Literal(ConansSegment).Literal(SearchSegment).Query("q", q)
                .Query("ignorecase", ignoreCase).Build(), GitLabJsonContext.Default.JsonElement, cancellationToken);
    }

    public Task<GitLabConanRevisionInfo> GetLatestRecipeRevisionAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRouteV2(projectId, packageName, packageVersion, packageUsername, packageChannel)
                .Literal(LatestSegment).Build(), GitLabJsonContext.Default.GitLabConanRevisionInfo, cancellationToken);
    }

    public Task<GitLabConanRecipeRevisions> GetRecipeRevisionsAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRouteV2(projectId, packageName, packageVersion, packageUsername, packageChannel)
                .Literal(RevisionsSegment).Build(), GitLabJsonContext.Default.GitLabConanRecipeRevisions,
            cancellationToken);
    }

    public Task DeleteRecipeRevisionAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision)
                .Build(), cancellationToken);
    }

    public Task<GitLabConanFileList> GetRecipeRevisionFilesAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision)
                .Literal(FilesSegment).Build(), GitLabJsonContext.Default.GitLabConanFileList, cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadRecipeRevisionFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision)
                .Literal(FilesSegment).Escaped(fileName).Build(), cancellationToken);
    }

    public Task AuthorizeRecipeRevisionFileUploadAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision)
                .Literal(FilesSegment).Escaped(fileName).Literal(AuthorizeSegment).Build(), cancellationToken);
    }

    public Task<GitLabConanRevisionInfo> GetLatestPackageRevisionAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision)
                .Literal(PackagesSegment).Escaped(conanPackageReference).Literal(LatestSegment).Build(),
            GitLabJsonContext.Default.GitLabConanRevisionInfo, cancellationToken);
    }

    public Task<GitLabConanPackageRevisions> GetPackageRevisionsAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision)
                .Literal(PackagesSegment).Escaped(conanPackageReference).Literal(RevisionsSegment).Build(),
            GitLabJsonContext.Default.GitLabConanPackageRevisions, cancellationToken);
    }

    public Task DeletePackageRevisionAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            PackageRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision, conanPackageReference, packageRevision).Build(), cancellationToken);
    }

    public Task<GitLabConanFileList> GetPackageRevisionFilesAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            PackageRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision, conanPackageReference, packageRevision).Literal(FilesSegment).Build(),
            GitLabJsonContext.Default.GitLabConanFileList, cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadPackageRevisionFileAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            PackageRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision, conanPackageReference, packageRevision).Literal(FilesSegment).Escaped(fileName).Build(),
            cancellationToken);
    }

    public Task AuthorizePackageRevisionFileUploadAsync(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            PackageRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                    recipeRevision, conanPackageReference, packageRevision).Literal(FilesSegment).Escaped(fileName)
                .Literal(AuthorizeSegment).Build(), cancellationToken);
    }

    public Task<JsonElement> SearchPackageReferencesByRecipeRevisionAsync(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel, recipeRevision)
                .Literal(SearchSegment).Build(), GitLabJsonContext.Default.JsonElement, cancellationToken);
    }

    public Task<JsonElement> SearchPackageReferencesForProjectV2Async(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RecipeRouteV2(projectId, packageName, packageVersion, packageUsername, packageChannel)
                .Literal(SearchSegment).Build(), GitLabJsonContext.Default.JsonElement, cancellationToken);
    }

    public Task<GitLabFileResponse> AuthenticateForProjectV2Async(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(ProjectV2Route(projectId).Literal(UsersSegment).Literal("authenticate").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> CheckCredentialsForProjectV2Async(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectV2Route(projectId).Literal(UsersSegment).Literal("check_credentials").Build(), cancellationToken);
    }

    /// <summary>The instance-wide Conan v1 base route: <c>packages/conan/v1</c>.</summary>
    private static GitLabRouteBuilder InstanceRoute()
    {
        return GitLabRouteBuilder.Create(PackagesSegment).Literal(ConanSegment).Literal(V1Segment);
    }

    /// <summary>The per-project Conan v1 base route: <c>projects/:id/packages/conan/v1</c>.</summary>
    private static GitLabRouteBuilder ProjectRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId)
            .Literal(PackagesSegment).Literal(ConanSegment).Literal(V1Segment);
    }

    /// <summary>
    ///     The per-project Conan v2 base route: <c>projects/:id/packages/conan/v2</c>. Conan v2 has no
    ///     instance-wide form.
    /// </summary>
    private static GitLabRouteBuilder ProjectV2Route(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId)
            .Literal(PackagesSegment).Literal(ConanSegment).Literal("v2");
    }

    /// <summary>
    ///     <c>packages/conan/v1/conans/:package_name/:package_version/:package_username/:package_channel</c>.
    ///     Every coordinate is caller-supplied free text - <c>package_username</c> commonly carries a
    ///     namespaced project path encoded with <c>+</c> - so each goes through <c>.Escaped(...)</c>.
    /// </summary>
    private static GitLabRouteBuilder RecipeRoute(string packageName, string packageVersion,
        string packageUsername, string packageChannel)
    {
        return InstanceRoute().Literal(ConansSegment)
            .Escaped(packageName).Escaped(packageVersion).Escaped(packageUsername).Escaped(packageChannel);
    }

    /// <summary>
    ///     <c>projects/:id/packages/conan/v1/conans/:package_name/:package_version/:package_username/:package_channel</c>.
    /// </summary>
    private static GitLabRouteBuilder RecipeRoute(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel)
    {
        return ProjectRoute(projectId).Literal(ConansSegment)
            .Escaped(packageName).Escaped(packageVersion).Escaped(packageUsername).Escaped(packageChannel);
    }

    /// <summary>Appends <c>packages/:conan_package_reference</c> to an instance-wide recipe route.</summary>
    private static GitLabRouteBuilder PackageRoute(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference)
    {
        return RecipeRoute(packageName, packageVersion, packageUsername, packageChannel)
            .Literal(PackagesSegment).Escaped(conanPackageReference);
    }

    /// <summary>Appends <c>packages/:conan_package_reference</c> to a per-project recipe route.</summary>
    private static GitLabRouteBuilder PackageRoute(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string conanPackageReference)
    {
        return RecipeRoute(projectId, packageName, packageVersion, packageUsername, packageChannel)
            .Literal(PackagesSegment).Escaped(conanPackageReference);
    }

    private static GitLabRouteBuilder RecipeFileRoute(string packageName, string packageVersion, string packageUsername,
        string packageChannel, string recipeRevision, string fileName)
    {
        return InstanceRoute().Literal(FilesSegment).Escaped(packageName).Escaped(packageVersion)
            .Escaped(packageUsername).Escaped(packageChannel).Escaped(recipeRevision).Literal(ExportSegment)
            .Escaped(fileName);
    }

    private static GitLabRouteBuilder RecipeFileRoute(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string fileName)
    {
        return ProjectRoute(projectId).Literal(FilesSegment).Escaped(packageName).Escaped(packageVersion)
            .Escaped(packageUsername).Escaped(packageChannel).Escaped(recipeRevision).Literal(ExportSegment)
            .Escaped(fileName);
    }

    private static GitLabRouteBuilder PackageFileRoute(string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName)
    {
        return InstanceRoute().Literal(FilesSegment).Escaped(packageName).Escaped(packageVersion)
            .Escaped(packageUsername).Escaped(packageChannel).Escaped(recipeRevision).Literal(PackageSegment)
            .Escaped(conanPackageReference).Escaped(packageRevision).Escaped(fileName);
    }

    private static GitLabRouteBuilder PackageFileRoute(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel, string recipeRevision, string conanPackageReference,
        string packageRevision, string fileName)
    {
        return ProjectRoute(projectId).Literal(FilesSegment).Escaped(packageName).Escaped(packageVersion)
            .Escaped(packageUsername).Escaped(packageChannel).Escaped(recipeRevision).Literal(PackageSegment)
            .Escaped(conanPackageReference).Escaped(packageRevision).Escaped(fileName);
    }

    private static GitLabRouteBuilder RecipeRouteV2(ProjectId projectId, string packageName, string packageVersion,
        string packageUsername, string packageChannel)
    {
        return ProjectV2Route(projectId).Literal(ConansSegment).Escaped(packageName).Escaped(packageVersion)
            .Escaped(packageUsername).Escaped(packageChannel);
    }

    private static GitLabRouteBuilder RecipeRevisionRoute(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision)
    {
        return RecipeRouteV2(projectId, packageName, packageVersion, packageUsername, packageChannel)
            .Literal(RevisionsSegment).Escaped(recipeRevision);
    }

    private static GitLabRouteBuilder PackageRevisionRoute(ProjectId projectId, string packageName,
        string packageVersion, string packageUsername, string packageChannel, string recipeRevision,
        string conanPackageReference, string packageRevision)
    {
        return RecipeRevisionRoute(projectId, packageName, packageVersion, packageUsername, packageChannel,
                recipeRevision).Literal(PackagesSegment).Escaped(conanPackageReference).Literal(RevisionsSegment)
            .Escaped(packageRevision);
    }
}