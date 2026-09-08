using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's Terraform Module Registry API - the group/instance-wide registry protocol endpoints
///     under <c>/packages/terraform/modules/v1/:module_namespace/:module_name/:module_system</c>, and the
///     project-scoped publish/download endpoints under
///     <c>/projects/:id/packages/terraform/modules/:module_name/:module_system</c>.
///     <para>
///         This is a different GitLab API area from <see cref="ITerraformStatesClient" />: that resource is
///         the Terraform remote-state backend (<c>/projects/:id/terraform/state</c>), while this one is a
///         package registry for publishing and consuming reusable Terraform modules. The two share no
///         routes or DTOs.
///     </para>
/// </summary>
public interface IPackagesTerraformModulesClient
{
    /// <summary>
    ///     Retrieves the metadata for the latest published version of a module, addressed by the group
    ///     namespace that owns it. GitLab's response is a deliberately pruned subset of the community
    ///     Terraform Module Registry Protocol schema - see <see cref="GitLabTerraformModule" />.
    /// </summary>
    /// <param name="moduleNamespace">The owning group, by numeric ID or full path.</param>
    /// <param name="moduleName">The module name.</param>
    /// <param name="moduleSystem">The module system (provider), for example <c>local</c> or <c>aws</c>.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the request to complete.</param>
    Task<GitLabTerraformModule> GetModuleAsync(GroupId moduleNamespace, string moduleName, string moduleSystem,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves the metadata for one specific published version of a module.</summary>
    /// <param name="moduleNamespace">The owning group, by numeric ID or full path.</param>
    /// <param name="moduleName">The module name.</param>
    /// <param name="moduleSystem">The module system (provider).</param>
    /// <param name="moduleVersion">The version to retrieve, for example <c>1.0.0</c>.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the request to complete.</param>
    Task<GitLabTerraformModule> GetModuleVersionAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, string moduleVersion, CancellationToken cancellationToken = default);

    /// <summary>Lists every published version of a module, along with each version's dependency metadata.</summary>
    /// <param name="moduleNamespace">The owning group, by numeric ID or full path.</param>
    /// <param name="moduleName">The module name.</param>
    /// <param name="moduleSystem">The module system (provider).</param>
    /// <param name="cancellationToken">A token to observe while waiting for the request to complete.</param>
    Task<GitLabTerraformModuleVersionList> ListModuleVersionsAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads the packaged archive (<c>.tgz</c>) for one specific module version directly, through the
    ///     group/instance-wide registry route.
    /// </summary>
    /// <param name="moduleNamespace">The owning group, by numeric ID or full path.</param>
    /// <param name="moduleName">The module name.</param>
    /// <param name="moduleSystem">The module system (provider).</param>
    /// <param name="moduleVersion">The version to download.</param>
    /// <remarks>
    ///     This is the GitLab-specific direct-download route (path segment <c>file</c>), distinct from the
    ///     Terraform Module Registry Protocol's <c>download</c> route wrapped by
    ///     <see cref="DownloadModuleVersionAsync(GroupId, string, string, string, CancellationToken)" /> - see
    ///     that method's remarks for how the two differ.
    /// </remarks>
    /// <param name="cancellationToken">A token to observe while waiting for the request to complete.</param>
    Task<GitLabFileResponse> DownloadModuleVersionFileAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, string moduleVersion, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the download location for the latest version of a module, through the Terraform Module
    ///     Registry Protocol's group/instance-wide <c>download</c> route.
    /// </summary>
    /// <param name="moduleNamespace">The owning group, by numeric ID or full path.</param>
    /// <param name="moduleName">The module name.</param>
    /// <param name="moduleSystem">The module system (provider).</param>
    /// <remarks>
    ///     GitLab answers this protocol route with <c>204 No Content</c> and the actual archive location in
    ///     an <c>X-Terraform-Get</c> response header rather than a body; <see cref="IGitLabApiConnection" />
    ///     has no overload that exposes arbitrary response headers alongside a GET body, so the returned
    ///     <see cref="GitLabFileResponse" /> carries only the status code and an empty body. Prefer
    ///     <see cref="DownloadModuleVersionFileAsync" /> (or, for the latest version, resolve it first via
    ///     <see cref="GetModuleAsync" />) when the archive bytes themselves are what's needed.
    /// </remarks>
    /// <param name="cancellationToken">A token to observe while waiting for the request to complete.</param>
    Task<GitLabFileResponse> DownloadModuleAsync(GroupId moduleNamespace, string moduleName, string moduleSystem,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the download location for one specific module version, through the Terraform Module
    ///     Registry Protocol's group/instance-wide <c>download</c> route.
    /// </summary>
    /// <param name="moduleNamespace">The owning group, by numeric ID or full path.</param>
    /// <param name="moduleName">The module name.</param>
    /// <param name="moduleSystem">The module system (provider).</param>
    /// <param name="moduleVersion">The version to resolve.</param>
    /// <remarks>
    ///     See <see cref="DownloadModuleAsync(GroupId, string, string, CancellationToken)" /> for the same
    ///     <c>X-Terraform-Get</c> header caveat; use <see cref="DownloadModuleVersionFileAsync" /> instead to
    ///     download the archive bytes directly.
    /// </remarks>
    /// <param name="cancellationToken">A token to observe while waiting for the request to complete.</param>
    Task<GitLabFileResponse> DownloadModuleVersionAsync(GroupId moduleNamespace, string moduleName,
        string moduleSystem, string moduleVersion, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads the packaged archive (<c>.tgz</c>) for the latest version of a module, through the
    ///     project-scoped route.
    /// </summary>
    /// <param name="projectId">The project the module is published under.</param>
    /// <param name="moduleName">The module name.</param>
    /// <param name="moduleSystem">The module system (provider).</param>
    /// <param name="terraformGet">
    ///     Sets the <c>terraform-get</c> query flag Terraform itself sends when resolving this route as a
    ///     module source address. Leave this null for a normal direct download: GitLab then answers with the
    ///     archive body. Setting it true asks for the Module Registry Protocol's redirect behavior instead
    ///     (<c>204 No Content</c> plus an <c>X-Terraform-Get</c> header), which this method cannot surface -
    ///     the returned <see cref="GitLabFileResponse" /> would carry only the status code and an empty body.
    /// </param>
    /// <param name="cancellationToken">A token to observe while waiting for the request to complete.</param>
    Task<GitLabFileResponse> DownloadLatestModuleAsync(ProjectId projectId, string moduleName, string moduleSystem,
        bool? terraformGet = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads the packaged archive (<c>.tgz</c>) for one specific module version, through the
    ///     project-scoped route.
    /// </summary>
    /// <param name="projectId">The project the module is published under.</param>
    /// <param name="moduleName">The module name.</param>
    /// <param name="moduleSystem">The module system (provider).</param>
    /// <param name="moduleVersion">The version to download.</param>
    /// <param name="terraformGet">
    ///     See <see cref="DownloadLatestModuleAsync(ProjectId, string, string, bool?, CancellationToken)" />
    ///     - the same caveat about the redirect mode applies here.
    /// </param>
    /// <param name="cancellationToken">A token to observe while waiting for the request to complete.</param>
    Task<GitLabFileResponse> DownloadModuleVersionAsync(ProjectId projectId, string moduleName, string moduleSystem,
        string moduleVersion, bool? terraformGet = null, CancellationToken cancellationToken = default);

    /// <summary>Publishes a Terraform module package file (<c>.tgz</c>) under a project.</summary>
    /// <param name="projectId">The project to publish the module under.</param>
    /// <param name="moduleName">The module name.</param>
    /// <param name="moduleSystem">The module system (provider).</param>
    /// <param name="moduleVersion">The version being published.</param>
    /// <param name="file">The module archive to upload.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the request to complete.</param>
    Task<GitLabTerraformModuleUploadResult> UploadModuleFileAsync(ProjectId projectId, string moduleName,
        string moduleSystem, string moduleVersion, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     The Workhorse authorize step GitLab requires immediately before uploading a Terraform module
    ///     package file to this same route.
    /// </summary>
    /// <param name="projectId">The project the module is published under.</param>
    /// <param name="moduleName">The module name.</param>
    /// <param name="moduleSystem">The module system (provider).</param>
    /// <param name="moduleVersion">The version being published.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the request to complete.</param>
    Task AuthorizeModuleFileUploadAsync(ProjectId projectId, string moduleName, string moduleSystem,
        string moduleVersion, CancellationToken cancellationToken = default);
}