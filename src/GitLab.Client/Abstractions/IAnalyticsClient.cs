using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's reporting surface: group activity counts (<c>/analytics/group_activity/*</c>),
///     a project's deployment frequency (<c>/projects/:id/analytics/deployment_frequency</c>), code
///     review analytics (<c>/analytics/code_review</c>) and DORA metrics
///     (<c>/groups/:id/dora/metrics</c>, <c>/projects/:id/dora/metrics</c>). Entirely read-only.
///     <para>
///         The DORA metrics endpoints declare no response schema in GitLab's OpenAPI document, so they
///         are surfaced as a raw <see cref="JsonElement" /> rather than an invented shape.
///     </para>
/// </summary>
public interface IAnalyticsClient
{
    /// <summary>The count of issues recently created in a group, identified by its full path.</summary>
    Task<GitLabGroupIssuesCount> GetGroupActivityIssuesCountAsync(string groupPath,
        CancellationToken cancellationToken = default);

    /// <summary>The count of merge requests recently created in a group, identified by its full path.</summary>
    Task<GitLabGroupMergeRequestsCount> GetGroupActivityMergeRequestsCountAsync(string groupPath,
        CancellationToken cancellationToken = default);

    /// <summary>The count of members recently added to a group, identified by its full path.</summary>
    Task<GitLabGroupNewMembersCount> GetGroupActivityNewMembersCountAsync(string groupPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the project's deployment-frequency series for one environment over a date range.
    /// </summary>
    IAsyncEnumerable<GitLabDeploymentFrequency> ListDeploymentFrequencyAsync(ProjectId projectId,
        string environment, string from, DeploymentFrequencyListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams code-review metrics for the open merge requests of a project, identified by its numeric id.</summary>
    IAsyncEnumerable<GitLabCodeReviewAnalyticsItem> ListCodeReviewAnalyticsAsync(long projectId,
        CodeReviewAnalyticsListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a group-level DORA metric series - <c>metric</c> is one of
    ///     <c>deployment_frequency</c>, <c>lead_time_for_changes</c>, <c>time_to_restore_service</c> or
    ///     <c>change_failure_rate</c>.
    /// </summary>
    Task<JsonElement> GetGroupDoraMetricsAsync(GroupId groupId, string metric, DoraMetricsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>The project-level counterpart of <see cref="GetGroupDoraMetricsAsync" />.</summary>
    Task<JsonElement> GetProjectDoraMetricsAsync(ProjectId projectId, string metric,
        DoraMetricsOptions? options = null, CancellationToken cancellationToken = default);
}