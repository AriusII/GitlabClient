using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class PackagesDebianRepository(IGitLabApiConnection connection) : IPackagesDebianRepository
{
    // ---- Distributions (project scope) ----

    public IAsyncEnumerable<GitLabDebianDistribution> ListDistributionsForProjectAsync(ProjectId projectId,
        DebianDistributionListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            ProjectDistributionsRoute(projectId).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabDebianDistributionArray,
            cancellationToken);
    }

    public Task<GitLabDebianDistribution> CreateDistributionForProjectAsync(ProjectId projectId,
        CreateDebianDistributionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            ProjectDistributionsRoute(projectId).Build(),
            request,
            GitLabJsonContext.Default.CreateDebianDistributionRequest,
            GitLabJsonContext.Default.GitLabDebianDistribution,
            cancellationToken);
    }

    public Task<GitLabDebianDistribution> GetDistributionForProjectAsync(ProjectId projectId, string codename,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectDistributionsRoute(projectId).Escaped(codename).Build(),
            GitLabJsonContext.Default.GitLabDebianDistribution,
            cancellationToken);
    }

    public Task<GitLabDebianDistribution> UpdateDistributionForProjectAsync(ProjectId projectId, string codename,
        UpdateDebianDistributionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectDistributionsRoute(projectId).Escaped(codename).Build(),
            request,
            GitLabJsonContext.Default.UpdateDebianDistributionRequest,
            GitLabJsonContext.Default.GitLabDebianDistribution,
            cancellationToken);
    }

    public Task DeleteDistributionForProjectAsync(ProjectId projectId, string codename,
        DeleteDebianDistributionOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            ProjectDistributionsRoute(projectId).Escaped(codename).QueryFrom(options).Build(),
            cancellationToken);
    }

    public Task<GitLabDebianDistribution> GetDistributionKeyForProjectAsync(ProjectId projectId, string codename,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectDistributionsRoute(projectId).Escaped(codename).Literal("key.asc").Build(),
            GitLabJsonContext.Default.GitLabDebianDistribution,
            cancellationToken);
    }

    // ---- Distributions (group scope) ----

    public IAsyncEnumerable<GitLabDebianDistribution> ListDistributionsForGroupAsync(GroupId groupId,
        DebianDistributionListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GroupDistributionsRoute(groupId).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabDebianDistributionArray,
            cancellationToken);
    }

    public Task<GitLabDebianDistribution> CreateDistributionForGroupAsync(GroupId groupId,
        CreateDebianDistributionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GroupDistributionsRoute(groupId).Build(),
            request,
            GitLabJsonContext.Default.CreateDebianDistributionRequest,
            GitLabJsonContext.Default.GitLabDebianDistribution,
            cancellationToken);
    }

    public Task<GitLabDebianDistribution> GetDistributionForGroupAsync(GroupId groupId, string codename,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupDistributionsRoute(groupId).Escaped(codename).Build(),
            GitLabJsonContext.Default.GitLabDebianDistribution,
            cancellationToken);
    }

    public Task<GitLabDebianDistribution> UpdateDistributionForGroupAsync(GroupId groupId, string codename,
        UpdateDebianDistributionRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GroupDistributionsRoute(groupId).Escaped(codename).Build(),
            request,
            GitLabJsonContext.Default.UpdateDebianDistributionRequest,
            GitLabJsonContext.Default.GitLabDebianDistribution,
            cancellationToken);
    }

    public Task DeleteDistributionForGroupAsync(GroupId groupId, string codename,
        DeleteDebianDistributionOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GroupDistributionsRoute(groupId).Escaped(codename).QueryFrom(options).Build(),
            cancellationToken);
    }

    public Task<GitLabDebianDistribution> GetDistributionKeyForGroupAsync(GroupId groupId, string codename,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupDistributionsRoute(groupId).Escaped(codename).Literal("key.asc").Build(),
            GitLabJsonContext.Default.GitLabDebianDistribution,
            cancellationToken);
    }

    // ---- APT metadata tree (project scope) ----

    public Task<GitLabFileResponse> GetInReleaseForProjectAsync(ProjectId projectId, string distribution,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectDistsRoute(projectId, distribution).Literal("InRelease").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetReleaseForProjectAsync(ProjectId projectId, string distribution,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectDistsRoute(projectId, distribution).Literal("Release").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetReleaseSignatureForProjectAsync(ProjectId projectId, string distribution,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ProjectDistsRoute(projectId, distribution).Literal("Release.gpg").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetBinaryPackagesIndexForProjectAsync(ProjectId projectId, string distribution,
        string component, string architecture, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            BinaryRoute(ProjectDistsRoute(projectId, distribution), component, architecture).Literal("Packages")
                .Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetBinaryPackagesIndexByHashForProjectAsync(ProjectId projectId,
        string distribution, string component, string architecture, string fileSha256,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ByHashRoute(BinaryRoute(ProjectDistsRoute(projectId, distribution), component, architecture),
                fileSha256).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetInstallerBinaryPackagesIndexForProjectAsync(ProjectId projectId,
        string distribution, string component, string architecture, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            InstallerBinaryRoute(ProjectDistsRoute(projectId, distribution), component, architecture)
                .Literal("Packages").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetInstallerBinaryPackagesIndexByHashForProjectAsync(ProjectId projectId,
        string distribution, string component, string architecture, string fileSha256,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ByHashRoute(
                InstallerBinaryRoute(ProjectDistsRoute(projectId, distribution), component, architecture),
                fileSha256).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetSourcePackagesIndexForProjectAsync(ProjectId projectId, string distribution,
        string component, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            SourceRoute(ProjectDistsRoute(projectId, distribution), component).Literal("Sources").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetSourcePackagesIndexByHashForProjectAsync(ProjectId projectId,
        string distribution, string component, string fileSha256, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ByHashRoute(SourceRoute(ProjectDistsRoute(projectId, distribution), component), fileSha256).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadPackageFileForProjectAsync(ProjectId projectId, string distribution,
        string letter, string packageName, string packageVersion, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("debian")
                .Literal("pool").Escaped(distribution).Escaped(letter).Escaped(packageName)
                .Escaped(packageVersion).Escaped(fileName).Build(),
            cancellationToken);
    }

    // ---- APT metadata tree (group scope) ----

    public Task<GitLabFileResponse> GetInReleaseForGroupAsync(GroupId groupId, string distribution,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GroupDistsRoute(groupId, distribution).Literal("InRelease").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetReleaseForGroupAsync(GroupId groupId, string distribution,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GroupDistsRoute(groupId, distribution).Literal("Release").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetReleaseSignatureForGroupAsync(GroupId groupId, string distribution,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GroupDistsRoute(groupId, distribution).Literal("Release.gpg").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetBinaryPackagesIndexForGroupAsync(GroupId groupId, string distribution,
        string component, string architecture, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            BinaryRoute(GroupDistsRoute(groupId, distribution), component, architecture).Literal("Packages")
                .Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetBinaryPackagesIndexByHashForGroupAsync(GroupId groupId, string distribution,
        string component, string architecture, string fileSha256, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ByHashRoute(BinaryRoute(GroupDistsRoute(groupId, distribution), component, architecture), fileSha256)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetInstallerBinaryPackagesIndexForGroupAsync(GroupId groupId,
        string distribution, string component, string architecture, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            InstallerBinaryRoute(GroupDistsRoute(groupId, distribution), component, architecture)
                .Literal("Packages").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetInstallerBinaryPackagesIndexByHashForGroupAsync(GroupId groupId,
        string distribution, string component, string architecture, string fileSha256,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ByHashRoute(InstallerBinaryRoute(GroupDistsRoute(groupId, distribution), component, architecture),
                fileSha256).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetSourcePackagesIndexForGroupAsync(GroupId groupId, string distribution,
        string component, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            SourceRoute(GroupDistsRoute(groupId, distribution), component).Literal("Sources").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetSourcePackagesIndexByHashForGroupAsync(GroupId groupId, string distribution,
        string component, string fileSha256, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            ByHashRoute(SourceRoute(GroupDistsRoute(groupId, distribution), component), fileSha256).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadPackageFileForGroupAsync(GroupId groupId, string distribution,
        long projectId, string letter, string packageName, string packageVersion, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("-").Literal("packages").Literal("debian")
                .Literal("pool").Escaped(distribution).Segment(projectId).Escaped(letter).Escaped(packageName)
                .Escaped(packageVersion).Escaped(fileName).Build(),
            cancellationToken);
    }

    // ---- Package upload (project scope) ----

    public Task AuthorizePackageUploadAsync(ProjectId projectId, string fileName,
        AuthorizeDebianPackageUploadRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("debian")
                .Escaped(fileName).Literal("authorize").Build(),
            request,
            GitLabJsonContext.Default.AuthorizeDebianPackageUploadRequest,
            cancellationToken);
    }

    // ---- Route builders ----

    private static GitLabRouteBuilder ProjectDistributionsRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("debian_distributions");
    }

    private static GitLabRouteBuilder GroupDistributionsRoute(GroupId groupId)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("-").Literal("debian_distributions");
    }

    /// <summary>
    ///     The Debian codename/suite that opens every route under <c>packages/debian/dists</c>. Caller-
    ///     supplied free text, so it is <c>Escaped</c>, never <c>Literal</c>.
    /// </summary>
    private static GitLabRouteBuilder ProjectDistsRoute(ProjectId projectId, string distribution)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("debian")
            .Literal("dists").Escaped(distribution);
    }

    private static GitLabRouteBuilder GroupDistsRoute(GroupId groupId, string distribution)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("-").Literal("packages")
            .Literal("debian").Literal("dists").Escaped(distribution);
    }

    /// <summary>
    ///     Appends <c>/:component/binary-:architecture</c>. Both are caller-supplied free text and each is
    ///     its own path segment, so each gets its own <c>Escaped</c> call - the architecture is folded into
    ///     one <c>Escaped("binary-" + architecture)</c> because <c>binary-{architecture}</c> is a single
    ///     path segment in GitLab's route template, not two.
    /// </summary>
    private static GitLabRouteBuilder BinaryRoute(GitLabRouteBuilder dists, string component, string architecture)
    {
        return dists.Escaped(component).Escaped("binary-" + architecture);
    }

    private static GitLabRouteBuilder InstallerBinaryRoute(GitLabRouteBuilder dists, string component,
        string architecture)
    {
        return dists.Escaped(component).Literal("debian-installer").Escaped("binary-" + architecture);
    }

    private static GitLabRouteBuilder SourceRoute(GitLabRouteBuilder dists, string component)
    {
        return dists.Escaped(component).Literal("source");
    }

    private static GitLabRouteBuilder ByHashRoute(GitLabRouteBuilder indexRoute, string fileSha256)
    {
        return indexRoute.Literal("by-hash").Literal("SHA256").Escaped(fileSha256);
    }
}