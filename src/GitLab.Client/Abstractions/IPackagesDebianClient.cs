using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's Debian package registry - an APT-compatible repository per project or group.
///     <para>
///         The API has two shapes. <c>debian_distributions</c> (both project- and group-scoped) is an
///         ordinary JSON CRUD resource, so those members return the typed
///         <see cref="Models.GitLabDebianDistribution" />. Everything under <c>packages/debian</c> is the
///         APT protocol itself - the <c>Release</c>/<c>InRelease</c>/<c>Packages</c>/<c>Sources</c> index
///         files an <c>apt</c> client fetches and the <c>.deb</c>/<c>.dsc</c>/<c>.tar</c> files in the
///         pool - and every one of those is raw bytes, never JSON, so those members return
///         <see cref="GitLabFileResponse" />.
///     </para>
///     <para>
///         Distribution codenames, Debian component names and file names are caller-supplied free text
///         that legally contains dots (<c>bullseye-security</c>, <c>example_1.0.0~alpha2_amd64.deb</c>);
///         every implementation percent-encodes each one as its own path segment.
///     </para>
/// </summary>
public interface IPackagesDebianClient
{
    // ---- Distributions (project scope) ----

    /// <summary>Streams every Debian distribution on a project.</summary>
    IAsyncEnumerable<GitLabDebianDistribution> ListDistributionsForProjectAsync(ProjectId projectId,
        DebianDistributionListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a Debian distribution on a project. Only <see cref="CreateDebianDistributionRequest.Codename" /> is
    ///     required.
    /// </summary>
    Task<GitLabDebianDistribution> CreateDistributionForProjectAsync(ProjectId projectId,
        CreateDebianDistributionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets one project distribution by its codename.</summary>
    Task<GitLabDebianDistribution> GetDistributionForProjectAsync(ProjectId projectId, string codename,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a project distribution. The codename itself is immutable once created.</summary>
    Task<GitLabDebianDistribution> UpdateDistributionForProjectAsync(ProjectId projectId, string codename,
        UpdateDebianDistributionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a project distribution. GitLab answers <c>202 Accepted</c>: deletion happens asynchronously.</summary>
    Task DeleteDistributionForProjectAsync(ProjectId projectId, string codename,
        DeleteDebianDistributionOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a project distribution's signing key metadata.</summary>
    Task<GitLabDebianDistribution> GetDistributionKeyForProjectAsync(ProjectId projectId, string codename,
        CancellationToken cancellationToken = default);

    // ---- Distributions (group scope) ----

    /// <summary>Streams every Debian distribution on a group.</summary>
    IAsyncEnumerable<GitLabDebianDistribution> ListDistributionsForGroupAsync(GroupId groupId,
        DebianDistributionListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a Debian distribution on a group. Only <see cref="CreateDebianDistributionRequest.Codename" /> is
    ///     required.
    /// </summary>
    Task<GitLabDebianDistribution> CreateDistributionForGroupAsync(GroupId groupId,
        CreateDebianDistributionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets one group distribution by its codename.</summary>
    Task<GitLabDebianDistribution> GetDistributionForGroupAsync(GroupId groupId, string codename,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a group distribution. The codename itself is immutable once created.</summary>
    Task<GitLabDebianDistribution> UpdateDistributionForGroupAsync(GroupId groupId, string codename,
        UpdateDebianDistributionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a group distribution. GitLab answers <c>202 Accepted</c>: deletion happens asynchronously.</summary>
    Task DeleteDistributionForGroupAsync(GroupId groupId, string codename,
        DeleteDebianDistributionOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a group distribution's signing key metadata.</summary>
    Task<GitLabDebianDistribution> GetDistributionKeyForGroupAsync(GroupId groupId, string codename,
        CancellationToken cancellationToken = default);

    // ---- APT metadata tree (project scope) ----

    /// <summary>Downloads a project distribution's signed <c>InRelease</c> file.</summary>
    Task<GitLabFileResponse> GetInReleaseForProjectAsync(ProjectId projectId, string distribution,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a project distribution's unsigned <c>Release</c> file.</summary>
    Task<GitLabFileResponse> GetReleaseForProjectAsync(ProjectId projectId, string distribution,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a project distribution's detached <c>Release.gpg</c> signature.</summary>
    Task<GitLabFileResponse> GetReleaseSignatureForProjectAsync(ProjectId projectId, string distribution,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a project component/architecture's <c>Packages</c> binary index.</summary>
    Task<GitLabFileResponse> GetBinaryPackagesIndexForProjectAsync(ProjectId projectId, string distribution,
        string component, string architecture, CancellationToken cancellationToken = default);

    /// <summary>Downloads a project binary index by its SHA256 content hash, for reproducible <c>by-hash</c> fetches.</summary>
    Task<GitLabFileResponse> GetBinaryPackagesIndexByHashForProjectAsync(ProjectId projectId, string distribution,
        string component, string architecture, string fileSha256, CancellationToken cancellationToken = default);

    /// <summary>Downloads a project component/architecture's <c>debian-installer</c> (udeb) <c>Packages</c> index.</summary>
    Task<GitLabFileResponse> GetInstallerBinaryPackagesIndexForProjectAsync(ProjectId projectId,
        string distribution, string component, string architecture, CancellationToken cancellationToken = default);

    /// <summary>Downloads a project <c>debian-installer</c> index by its SHA256 content hash.</summary>
    Task<GitLabFileResponse> GetInstallerBinaryPackagesIndexByHashForProjectAsync(ProjectId projectId,
        string distribution, string component, string architecture, string fileSha256,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a project component's <c>Sources</c> index.</summary>
    Task<GitLabFileResponse> GetSourcePackagesIndexForProjectAsync(ProjectId projectId, string distribution,
        string component, CancellationToken cancellationToken = default);

    /// <summary>Downloads a project source index by its SHA256 content hash.</summary>
    Task<GitLabFileResponse> GetSourcePackagesIndexByHashForProjectAsync(ProjectId projectId, string distribution,
        string component, string fileSha256, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one package file from a project's Debian pool - the actual <c>.deb</c>/<c>.udeb</c>/
    ///     <c>.dsc</c>/<c>.tar</c> bytes at <c>pool/:distribution/:letter/:package_name/:package_version/:file_name</c>.
    /// </summary>
    Task<GitLabFileResponse> DownloadPackageFileForProjectAsync(ProjectId projectId, string distribution,
        string letter, string packageName, string packageVersion, string fileName,
        CancellationToken cancellationToken = default);

    // ---- APT metadata tree (group scope) ----

    /// <summary>Downloads a group distribution's signed <c>InRelease</c> file.</summary>
    Task<GitLabFileResponse> GetInReleaseForGroupAsync(GroupId groupId, string distribution,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a group distribution's unsigned <c>Release</c> file.</summary>
    Task<GitLabFileResponse> GetReleaseForGroupAsync(GroupId groupId, string distribution,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a group distribution's detached <c>Release.gpg</c> signature.</summary>
    Task<GitLabFileResponse> GetReleaseSignatureForGroupAsync(GroupId groupId, string distribution,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a group component/architecture's <c>Packages</c> binary index.</summary>
    Task<GitLabFileResponse> GetBinaryPackagesIndexForGroupAsync(GroupId groupId, string distribution,
        string component, string architecture, CancellationToken cancellationToken = default);

    /// <summary>Downloads a group binary index by its SHA256 content hash, for reproducible <c>by-hash</c> fetches.</summary>
    Task<GitLabFileResponse> GetBinaryPackagesIndexByHashForGroupAsync(GroupId groupId, string distribution,
        string component, string architecture, string fileSha256, CancellationToken cancellationToken = default);

    /// <summary>Downloads a group component/architecture's <c>debian-installer</c> (udeb) <c>Packages</c> index.</summary>
    Task<GitLabFileResponse> GetInstallerBinaryPackagesIndexForGroupAsync(GroupId groupId, string distribution,
        string component, string architecture, CancellationToken cancellationToken = default);

    /// <summary>Downloads a group <c>debian-installer</c> index by its SHA256 content hash.</summary>
    Task<GitLabFileResponse> GetInstallerBinaryPackagesIndexByHashForGroupAsync(GroupId groupId,
        string distribution, string component, string architecture, string fileSha256,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a group component's <c>Sources</c> index.</summary>
    Task<GitLabFileResponse> GetSourcePackagesIndexForGroupAsync(GroupId groupId, string distribution,
        string component, CancellationToken cancellationToken = default);

    /// <summary>Downloads a group source index by its SHA256 content hash.</summary>
    Task<GitLabFileResponse> GetSourcePackagesIndexByHashForGroupAsync(GroupId groupId, string distribution,
        string component, string fileSha256, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one package file from a group's Debian pool. Unlike the project-scoped download, the
    ///     route names which project under the group owns the file, since a group repository aggregates
    ///     packages published by several of its projects.
    /// </summary>
    Task<GitLabFileResponse> DownloadPackageFileForGroupAsync(GroupId groupId, string distribution, long projectId,
        string letter, string packageName, string packageVersion, string fileName,
        CancellationToken cancellationToken = default);

    // ---- Package upload (project scope) ----

    /// <summary>
    ///     Runs the pre-flight authorization check GitLab Workhorse requires before a Debian package upload.
    ///     There is deliberately no paired upload method here: GitLab answers the actual
    ///     <c>PUT /projects/:id/packages/debian/:file_name</c> upload with <c>201 Created</c> and no body,
    ///     a shape the current transport cannot express for a multipart <c>PUT</c> - see the resource's
    ///     implementation notes.
    /// </summary>
    Task AuthorizePackageUploadAsync(ProjectId projectId, string fileName,
        AuthorizeDebianPackageUploadRequest request, CancellationToken cancellationToken = default);
}