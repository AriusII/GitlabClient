using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Helm chart registry
///     (<c>/projects/:id/packages/helm/...</c>): builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls <see cref="IGitLabApiConnection" />.
///     Knows GitLab's wire format; nothing above this layer should build a route or touch
///     <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IPackagesHelmService), typeof(IPackagesHelmClient))]
internal interface IPackagesHelmRepository
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