using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Instance, sitting between the public <c>IInstanceClient</c>
///     controller and <c>IInstanceRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IInstanceService
{
    Task<GitLabAppearance> GetAppearanceAsync(CancellationToken cancellationToken = default);

    Task<GitLabApplicationSettings> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task<GitLabApplicationSettings> UpdateSettingsAsync(UpdateApplicationSettingsRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabApplicationStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    Task<GitLabMetadata> GetMetadataAsync(CancellationToken cancellationToken = default);

    Task<GitLabPlanLimits> GetPlanLimitsAsync(PlanLimitsOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabPlanLimits> UpdatePlanLimitsAsync(UpdatePlanLimitsRequest request,
        CancellationToken cancellationToken = default);
}