using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class InstanceRepository(IGitLabApiConnection connection) : IInstanceRepository
{
    // GitLab's own documentation for PUT /application/appearance sends the image fields as their own
    // multipart/form-data requests, one field per call, rather than mixing them into the JSON body
    // used for the text/boolean fields - see UpdateApplicationAppearanceRequest. Each constant below
    // is forced onto the outgoing GitLabFileUpload rather than trusted from the caller, whose default
    // FieldName is "file"; sending an image under the wrong field name is accepted with a 200 and
    // silently ignored.
    private const string LogoFieldName = "logo";
    private const string HeaderLogoFieldName = "header_logo";
    private const string PwaIconFieldName = "pwa_icon";
    private const string FaviconFieldName = "favicon";

    public Task<GitLabAppearance> GetAppearanceAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("application").Literal("appearance").Build(),
            GitLabJsonContext.Default.GitLabAppearance,
            cancellationToken);
    }

    public Task<GitLabAppearance> UpdateAppearanceAsync(UpdateApplicationAppearanceRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("application").Literal("appearance").Build(),
            request,
            GitLabJsonContext.Default.UpdateApplicationAppearanceRequest,
            GitLabJsonContext.Default.GitLabAppearance,
            cancellationToken);
    }

    public Task<GitLabAppearance> SetAppearanceLogoAsync(GitLabFileUpload logo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logo);

        return connection.PutFileAsync(
            GitLabRouteBuilder.Create("application").Literal("appearance").Build(),
            logo with { FieldName = LogoFieldName },
            null,
            GitLabJsonContext.Default.GitLabAppearance,
            cancellationToken);
    }

    public Task<GitLabAppearance> SetAppearanceHeaderLogoAsync(GitLabFileUpload headerLogo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(headerLogo);

        return connection.PutFileAsync(
            GitLabRouteBuilder.Create("application").Literal("appearance").Build(),
            headerLogo with { FieldName = HeaderLogoFieldName },
            null,
            GitLabJsonContext.Default.GitLabAppearance,
            cancellationToken);
    }

    public Task<GitLabAppearance> SetAppearancePwaIconAsync(GitLabFileUpload pwaIcon,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pwaIcon);

        return connection.PutFileAsync(
            GitLabRouteBuilder.Create("application").Literal("appearance").Build(),
            pwaIcon with { FieldName = PwaIconFieldName },
            null,
            GitLabJsonContext.Default.GitLabAppearance,
            cancellationToken);
    }

    public Task<GitLabAppearance> SetAppearanceFaviconAsync(GitLabFileUpload favicon,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(favicon);

        return connection.PutFileAsync(
            GitLabRouteBuilder.Create("application").Literal("appearance").Build(),
            favicon with { FieldName = FaviconFieldName },
            null,
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