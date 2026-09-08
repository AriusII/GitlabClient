using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Part E of the Integrations service seam - mirrors <c>IIntegrationsRepository</c>'s part-E members
///     1:1 (see that interface for the rationale); its implementation is generated.
/// </summary>
internal partial interface IIntegrationsService
{
    Task<GitLabIntegration> SetSquashTmAsync(ProjectId projectId, SquashTmSettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetTeamCityAsync(ProjectId projectId, TeamCitySettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetTelegramAsync(ProjectId projectId, TelegramSettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetUnifyCircuitAsync(ProjectId projectId, UnifyCircuitSettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetWebexTeamsAsync(ProjectId projectId, WebexTeamsSettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetYouTrackAsync(ProjectId projectId, YouTrackSettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetZentaoAsync(ProjectId projectId, ZentaoSettingsRequest settings,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> GetServiceAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default);

    Task DisableServiceAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default);
}