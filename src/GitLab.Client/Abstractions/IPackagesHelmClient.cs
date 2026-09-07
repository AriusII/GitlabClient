using GitLab.Client.Domain;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's Helm chart registry (<c>/projects/:id/packages/helm/...</c>) - uploading a chart to
///     a named channel and downloading a channel's charts and index.
///     <para>
///         A "channel" is Helm's own grouping concept (commonly <c>stable</c>), not a GitLab one; charts
///         published to different channels are indexed separately.
///     </para>
/// </summary>
public interface IPackagesHelmClient
{
    /// <summary>
    ///     Uploads a chart to a channel. GitLab answers <c>201 Created</c> with no body, so this method
    ///     completes once the upload is accepted rather than returning a package DTO.
    /// </summary>
    Task UploadChartAsync(ProjectId projectId, string channel, GitLabFileUpload chart,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     The workhorse pre-upload authorization check for <see cref="UploadChartAsync" />. Direct callers of
    ///     this client do not need it - it exists for workhorse-fronted deployments, not as a
    ///     precondition <see cref="UploadChartAsync" /> itself requires.
    /// </summary>
    Task AuthorizeChartUploadAsync(ProjectId projectId, string channel,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a chart's <c>.tgz</c> package. <paramref name="fileName" /> is the chart file name
    ///     without the <c>.tgz</c> extension; the route appends it.
    /// </summary>
    Task<GitLabFileResponse> DownloadChartAsync(ProjectId projectId, string channel, string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a channel's <c>index.yaml</c>, the Helm repository index listing every chart in it.</summary>
    Task<GitLabFileResponse> DownloadChartIndexAsync(ProjectId projectId, string channel,
        CancellationToken cancellationToken = default);
}