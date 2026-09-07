using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class InstanceRepository(IGitLabApiConnection connection) : IInstanceRepository
{
    public Task<GitLabAppearance> GetAppearanceAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("application").Literal("appearance").Build(),
            GitLabJsonContext.Default.GitLabAppearance,
            cancellationToken);
    }

    public Task<GitLabApplicationSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("application").Literal("settings").Build(),
            GitLabJsonContext.Default.GitLabApplicationSettings,
            cancellationToken);
    }

    public Task<GitLabApplicationSettings> UpdateSettingsAsync(UpdateApplicationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("application").Literal("settings").Build(),
            request,
            GitLabJsonContext.Default.UpdateApplicationSettingsRequest,
            GitLabJsonContext.Default.GitLabApplicationSettings,
            cancellationToken);
    }

    public Task<GitLabApplicationStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("application").Literal("statistics").Build(),
            GitLabJsonContext.Default.GitLabApplicationStatistics,
            cancellationToken);
    }

    public Task<GitLabMetadata> GetMetadataAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("metadata").Build(),
            GitLabJsonContext.Default.GitLabMetadata,
            cancellationToken);
    }

    public Task<GitLabPlanLimits> GetPlanLimitsAsync(PlanLimitsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("application").Literal("plan_limits").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabPlanLimits,
            cancellationToken);
    }

    public Task<GitLabPlanLimits> UpdatePlanLimitsAsync(UpdatePlanLimitsRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("application").Literal("plan_limits").Build(),
            request,
            GitLabJsonContext.Default.UpdatePlanLimitsRequest,
            GitLabJsonContext.Default.GitLabPlanLimits,
            cancellationToken);
    }
}