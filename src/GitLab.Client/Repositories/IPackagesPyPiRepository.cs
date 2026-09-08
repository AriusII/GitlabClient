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

    Task<GitLabRedirectResponse> ForwardPackageFileAsync(ProjectId projectId, string packageName,
        string upstreamPath, CancellationToken cancellationToken = default);
}