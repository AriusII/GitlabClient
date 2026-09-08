using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Alert management" API area
///     (<c>/projects/:id/alert_management_alerts/:alert_iid/metric_images</c>) - the metric-image
///     screenshots and dashboard links a responder attaches to an alert for visual context.
///     <para>
///         The GraphQL API covers the rest of alert management (listing and updating alerts themselves,
///         todos, and notes); this REST area exists solely for the metric-image sub-resource.
///     </para>
/// </summary>
public interface IAlertManagementClient
{
    /// <summary>Streams every metric image attached to an alert.</summary>
    IAsyncEnumerable<GitLabMetricImage> ListMetricImagesAsync(ProjectId projectId, long alertIid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a metric image to an alert, optionally with a link to more detail elsewhere (an
    ///     external dashboard, say).
    /// </summary>
    /// <param name="projectId">The project the alert belongs to.</param>
    /// <param name="alertIid">The alert's project-scoped IID.</param>
    /// <param name="file">
    ///     The image file part. Its stream is read but never disposed, so the caller keeps ownership of it.
    /// </param>
    /// <param name="url">An optional link to more metric detail elsewhere.</param>
    /// <param name="caption">A caption for the image or for <paramref name="url" />.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabMetricImage> UploadMetricImageAsync(ProjectId projectId, long alertIid, GitLabFileUpload file,
        Uri? url = null, string? caption = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asks GitLab Workhorse to authorize an upload before it is sent, which is how a large image is
    ///     streamed straight to object storage instead of through Rails.
    ///     <para>
    ///         The reply is Workhorse's internal routing document, which the API does not declare a schema
    ///         for and only Workhorse itself consumes, so it is not surfaced here. Ordinary callers want
    ///         <see cref="UploadMetricImageAsync" />, which does the whole thing in one call.
    ///     </para>
    /// </summary>
    Task AuthorizeMetricImageUploadAsync(ProjectId projectId, long alertIid,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one metric image from an alert.</summary>
    Task DeleteMetricImageAsync(ProjectId projectId, long alertIid, long metricImageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a metric image's optional link. There is no endpoint to replace the image file itself -
    ///     delete and re-upload instead.
    /// </summary>
    Task<GitLabMetricImage> UpdateMetricImageAsync(ProjectId projectId, long alertIid, long metricImageId,
        UpdateMetricImageRequest request, CancellationToken cancellationToken = default);
}