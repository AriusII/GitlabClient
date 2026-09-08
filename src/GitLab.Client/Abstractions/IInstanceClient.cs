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

    /// <summary>
    ///     Updates the text and boolean fields of the instance's branding (<c>PUT /application/appearance</c>),
    ///     leaving unmentioned fields unchanged.
    ///     <para>
    ///         GitLab documents this endpoint as accepting either a plain JSON body for these fields or a
    ///         <c>multipart/form-data</c> body carrying one of its four image fields; the two are never
    ///         combined in GitLab's own examples. This method sends the JSON form. To replace one of the
    ///         instance's images, use <see cref="SetAppearanceLogoAsync" />, <see cref="SetAppearanceHeaderLogoAsync" />,
    ///         <see cref="SetAppearancePwaIconAsync" /> or <see cref="SetAppearanceFaviconAsync" /> instead - each
    ///         issues its own call to the same route, since a single <c>multipart/form-data</c> request can
    ///         carry only one file part at a time.
    ///     </para>
    /// </summary>
    Task<GitLabAppearance> UpdateAppearanceAsync(UpdateApplicationAppearanceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replaces the instance's sign-in / sign-up page logo (<c>PUT /application/appearance</c> as
    ///     <c>multipart/form-data</c>, field <c>logo</c>). The file is sent under that field name regardless
    ///     of <see cref="GitLabFileUpload.FieldName" />.
    /// </summary>
    Task<GitLabAppearance> SetAppearanceLogoAsync(GitLabFileUpload logo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replaces the instance's main-navigation-bar logo (<c>PUT /application/appearance</c> as
    ///     <c>multipart/form-data</c>, field <c>header_logo</c>). The file is sent under that field name
    ///     regardless of <see cref="GitLabFileUpload.FieldName" />.
    /// </summary>
    Task<GitLabAppearance> SetAppearanceHeaderLogoAsync(GitLabFileUpload headerLogo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replaces the instance's Progressive Web App icon (<c>PUT /application/appearance</c> as
    ///     <c>multipart/form-data</c>, field <c>pwa_icon</c>). The file is sent under that field name
    ///     regardless of <see cref="GitLabFileUpload.FieldName" />.
    /// </summary>
    Task<GitLabAppearance> SetAppearancePwaIconAsync(GitLabFileUpload pwaIcon,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replaces the instance's favicon (<c>PUT /application/appearance</c> as <c>multipart/form-data</c>,
    ///     field <c>favicon</c>). The file is sent under that field name regardless of
    ///     <see cref="GitLabFileUpload.FieldName" />.
    /// </summary>
    Task<GitLabAppearance> SetAppearanceFaviconAsync(GitLabFileUpload favicon,
        CancellationToken cancellationToken = default);

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