using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for AlertManagement, sitting between the public
///     <c>IAlertManagementClient</c> controller and <c>IAlertManagementRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IAlertManagementService
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