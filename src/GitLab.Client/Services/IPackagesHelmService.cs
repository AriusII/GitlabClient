using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for the Helm chart registry, sitting between the public
///     <c>IPackagesHelmClient</c> controller and <c>IPackagesHelmRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IPackagesHelmService
{
    Task UploadChartAsync(ProjectId projectId, string channel, GitLabFileUpload chart,
        CancellationToken cancellationToken = default);

    Task AuthorizeChartUploadAsync(ProjectId projectId, string channel,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadChartAsync(ProjectId projectId, string channel, string fileName,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadChartIndexAsync(ProjectId projectId, string channel,
        CancellationToken cancellationToken = default);
}