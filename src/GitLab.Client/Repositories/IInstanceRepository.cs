using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Instance resource - instance branding, application settings,
///     entity statistics, version metadata and plan limits. Builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IInstanceService), typeof(IInstanceClient))]
internal interface IInstanceRepository
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