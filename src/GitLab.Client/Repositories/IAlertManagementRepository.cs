using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the AlertManagement resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IAlertManagementService), typeof(IAlertManagementClient))]
internal interface IAlertManagementRepository
{
    IAsyncEnumerable<GitLabMetricImage> ListMetricImagesAsync(ProjectId projectId, long alertIid,
        CancellationToken cancellationToken = default);

    Task<GitLabMetricImage> UploadMetricImageAsync(ProjectId projectId, long alertIid, GitLabFileUpload file,
        Uri? url = null, string? caption = null, CancellationToken cancellationToken = default);

    Task AuthorizeMetricImageUploadAsync(ProjectId projectId, long alertIid,
        CancellationToken cancellationToken = default);

    Task DeleteMetricImageAsync(ProjectId projectId, long alertIid, long metricImageId,
        CancellationToken cancellationToken = default);

    Task<GitLabMetricImage> UpdateMetricImageAsync(ProjectId projectId, long alertIid, long metricImageId,
        UpdateMetricImageRequest request, CancellationToken cancellationToken = default);
}