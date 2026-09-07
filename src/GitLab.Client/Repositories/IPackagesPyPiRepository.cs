using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the PyPI package registry
///     (<c>/groups/:id/-/packages/pypi/...</c>, <c>/projects/:id/packages/pypi/...</c>): builds routes
///     via <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
/// <remarks>
///     <c>GET /projects/:id/packages/pypi/forward/:package_name/:upstream_path</c> (proxying a package
///     file from the upstream PyPI dependency-firewall target) is deliberately not wrapped here. Its
///     success response is a bare <c>302 Found</c> redirect with no body, but
///     <see cref="IGitLabApiConnection" />'s handler pipeline runs with <c>AllowAutoRedirect = false</c>
///     and every transport method funnels non-2xx responses (a 3xx included) through the same
///     "throw the typed exception" path - there is no method that would hand a caller the
///     <c>Location</c> header instead of throwing. Wrapping it would need new transport surface, which is
///     centrally owned and out of scope here.
/// </remarks>
[GenerateClientLayers(typeof(IPackagesPyPiService), typeof(IPackagesPyPiClient))]
internal interface IPackagesPyPiRepository
{
    Task<GitLabFileResponse> DownloadFileForGroupAsync(GroupId groupId, string sha256, string fileIdentifier,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSimpleIndexForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSimplePackageForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default);

    Task UploadAsync(ProjectId projectId, GitLabFileUpload content, PyPiPackageUploadRequest metadata,
        CancellationToken cancellationToken = default);

    Task AuthorizeUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadFileAsync(ProjectId projectId, string sha256, string fileIdentifier,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSimpleIndexForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetSimplePackageForProjectAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);
}