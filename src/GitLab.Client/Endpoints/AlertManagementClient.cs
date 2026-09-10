using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class AlertManagementClient(IGitLabApiConnection connection) : IAlertManagementClient
{
    public IAsyncEnumerable<GitLabMetricImage> ListMetricImagesAsync(ProjectId projectId, long alertIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            MetricImagesRoute(projectId, alertIid).Build(),
            GitLabJsonContext.Default.GitLabMetricImageArray,
            cancellationToken);
    }

    public Task<GitLabMetricImage> UploadMetricImageAsync(ProjectId projectId, long alertIid, GitLabFileUpload file,
        Uri? url = null, string? caption = null, CancellationToken cancellationToken = default)
    {
        Dictionary<string, string>? formFields = null;

        if (url is not null || caption is not null)
        {
            formFields = new Dictionary<string, string>(StringComparer.Ordinal);

            if (url is not null)
            {
                formFields["url"] = url.ToString();
            }

            if (caption is not null)
            {
                formFields["url_text"] = caption;
            }
        }

        return connection.PostFileAsync(
            MetricImagesRoute(projectId, alertIid).Build(),
            file,
            formFields,
            GitLabJsonContext.Default.GitLabMetricImage,
            cancellationToken);
    }

    /// <summary>
    ///     Asks GitLab Workhorse to authorize the upload before it is sent. The reply is Workhorse's own
    ///     internal routing document, which the spec declares no schema for and only Workhorse itself
    ///     consumes, so it is not surfaced here - callers doing an ordinary upload want
    ///     <see cref="UploadMetricImageAsync" />, which does the whole thing in one call.
    /// </summary>
    public Task AuthorizeMetricImageUploadAsync(ProjectId projectId, long alertIid,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            MetricImagesRoute(projectId, alertIid).Literal("authorize").Build(),
            cancellationToken);
    }

    public Task DeleteMetricImageAsync(ProjectId projectId, long alertIid, long metricImageId,
        CancellationToken cancellationToken = default)
    {
        // GitLab's spec marks this 204; the no-content path is tolerant of any body it might still send.
        return connection.DeleteAsync(
            MetricImagesRoute(projectId, alertIid).Segment(metricImageId).Build(),
            cancellationToken);
    }

    public Task<GitLabMetricImage> UpdateMetricImageAsync(ProjectId projectId, long alertIid, long metricImageId,
        UpdateMetricImageRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutMultipartFormAsync(
            MetricImagesRoute(projectId, alertIid).Segment(metricImageId).Build(),
            request,
            GitLabJsonContext.Default.UpdateMetricImageRequest,
            GitLabJsonContext.Default.GitLabMetricImage,
            cancellationToken);
    }

    private static GitLabRouteBuilder MetricImagesRoute(ProjectId projectId, long alertIid)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("alert_management_alerts")
            .Segment(alertIid).Literal("metric_images");
    }
}