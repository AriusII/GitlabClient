using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's PyPI package registry - the group-scoped Simple-index endpoints
///     (<c>/groups/:id/-/packages/pypi/...</c>) plus the project-scoped upload, download and
///     Simple-index endpoints (<c>/projects/:id/packages/pypi/...</c>) that <c>pip</c> and
///     <c>twine</c> speak directly.
///     <para>
///         Every read here returns a <see cref="GitLabFileResponse" /> rather than a typed DTO: the
///         Simple-index endpoints answer with an HTML page (PEP 503's "Simple Repository API" format),
///         not JSON, so the caller parses the body as a PyPI client would.
///     </para>
/// </summary>
public interface IPackagesPyPiClient
{
    /// <summary>
    ///     Downloads a package file from a group-level PyPI registry. <paramref name="sha256" /> and
    ///     <paramref name="fileIdentifier" /> come from the Simple-index page's file links.
    /// </summary>
    Task<GitLabFileResponse> DownloadFileForGroupAsync(GroupId groupId, string sha256, string fileIdentifier,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the PEP 503 Simple-index page listing every package in the group.</summary>
    Task<GitLabFileResponse> GetSimpleIndexForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the PEP 503 Simple-index page listing every version of one package in the group.</summary>
    Task<GitLabFileResponse> GetSimplePackageForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a PyPI package (the shape <c>twine upload</c> speaks). GitLab answers
    ///     <c>201 Created</c> with no body, so this method completes once the upload is accepted rather
    ///     than returning a package DTO.
    /// </summary>
    Task UploadAsync(ProjectId projectId, GitLabFileUpload content, PyPiPackageUploadRequest metadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     The workhorse pre-upload authorization check for <see cref="UploadAsync" />. Direct callers of
    ///     this client do not need it - it exists for workhorse-fronted deployments, not as a
    ///     precondition <see cref="UploadAsync" /> itself requires.
    /// </summary>
    Task AuthorizeUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a package file from a project-level PyPI registry. <paramref name="sha256" /> and
    ///     <paramref name="fileIdentifier" /> come from the Simple-index page's file links.
    /// </summary>
    Task<GitLabFileResponse> DownloadFileAsync(ProjectId projectId, string sha256, string fileIdentifier,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the PEP 503 Simple-index page listing every package in the project.</summary>
    Task<GitLabFileResponse> GetSimpleIndexForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the PEP 503 Simple-index page listing every version of one package in the project.</summary>
    Task<GitLabFileResponse> GetSimplePackageForProjectAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Resolves a package file proxied through GitLab's PyPI Dependency Firewall. GitLab does not stream
    ///     the file itself - it enforces the firewall policy, then answers with a <c>302 Found</c> pointing
    ///     at the real upstream artifact, which is why this returns a <see cref="GitLabRedirectResponse" />
    ///     rather than a <see cref="GitLabFileResponse" />. <paramref name="upstreamPath" /> comes verbatim
    ///     from the link GitLab already rewrote into the Simple-index page - construct it by hand and GitLab
    ///     will 404, since it is not just the upstream URL's path.
    /// </summary>
    Task<GitLabRedirectResponse> ForwardPackageFileAsync(ProjectId projectId, string packageName,
        string upstreamPath, CancellationToken cancellationToken = default);
}