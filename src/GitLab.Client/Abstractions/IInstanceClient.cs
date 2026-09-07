using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's instance-level informational and administrative surface: branding
///     (<c>/application/appearance</c>), application settings (<c>/application/settings</c>), entity
///     statistics (<c>/application/statistics</c>), version metadata (<c>/metadata</c>) and billing-plan
///     limits (<c>/application/plan_limits</c>). With the exception of <see cref="GetMetadataAsync" />,
///     every operation here requires instance administrator rights.
/// </summary>
public interface IInstanceClient
{
    /// <summary>Gets the instance-wide branding shown on the sign-in and sign-up pages.</summary>
    Task<GitLabAppearance> GetAppearanceAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the current instance-wide application settings.</summary>
    Task<GitLabApplicationSettings> GetSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the instance-wide application settings, leaving unmentioned settings unchanged.</summary>
    Task<GitLabApplicationSettings> UpdateSettingsAsync(UpdateApplicationSettingsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Gets instance-wide entity counts (projects, users, issues, and so on).</summary>
    Task<GitLabApplicationStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets this instance's version, revision and edition. The only operation on this client that
    ///     does not require administrator rights.
    /// </summary>
    Task<GitLabMetadata> GetMetadataAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the resource limits configured for a billing plan.</summary>
    Task<GitLabPlanLimits> GetPlanLimitsAsync(PlanLimitsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Changes the resource limits configured for a billing plan, leaving unmentioned limits unchanged.</summary>
    Task<GitLabPlanLimits> UpdatePlanLimitsAsync(UpdatePlanLimitsRequest request,
        CancellationToken cancellationToken = default);
}