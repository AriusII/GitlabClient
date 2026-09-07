using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     Recipe and package metadata (v1): search, snapshots, manifests, download URLs and upload URLs, at
///     both instance-wide and per-project scope. The other regions of this resource - raw file transfer,
///     the instance/credentials checks, and the Conan v2 protocol area - live in this same partial class,
///     split across <c>PackagesConanRepository.Files.cs</c>, <c>PackagesConanRepository.Instance.cs</c> and
///     <c>PackagesConanRepository.V2.cs</c>.
/// </summary>
internal sealed partial class PackagesConanRepository(IGitLabApiConnection connection) : IPackagesConanRepository
{
    private const string PackagesSegment = "packages";
    private const string ConanSegment = "conan";
    private const string V1Segment = "v1";
    private const string ConansSegment = "conans";

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
}